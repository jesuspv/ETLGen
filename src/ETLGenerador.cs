using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Generador de archivos de datos para los procesos ETL.
	/// </summary>
	class ETLGenerador
	{
		private string delim;
		private string sepCampos;
		private string sepFilas;
		private int    tamMax;

		private ArrayList candidatas;
		private ArrayList unicas;
		private ArrayList noNulos;
		private ArrayList candidatasGeneradas;
		private ArrayList unicasGeneradas;

		private uint numFilasDescartadas;

		// Lista de generadores
		private ArrayList generadores;

		// Configuración de la aplicación
		private ETLConfig configuracion;

		public ETLGenerador()
		{
			delim     = "\"";
			sepCampos = ";";
			sepFilas  = "\r\n";
			tamMax    = 50;

			candidatas = new ArrayList();
			unicas     = new ArrayList();
			noNulos    = new ArrayList();
			candidatasGeneradas = new ArrayList();
			unicasGeneradas     = new ArrayList();
			numFilasDescartadas = 0;

			configuracion = new ETLConfig();
			generadores   = new ArrayList();
		}

		/// <summary>
		/// Punto de entrada principal de la aplicación.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// código de salida
			int codigoSalida = 0;

			try
			{
				if (args.Length != 3 && args.Length != 0)
				{
					throw new ArgumentException("Uso: ETLGen <muestras> <def.xml> <config.xml>\n");
				}
				else if (args.Length == 0)
				{
					new EliminadorCabeceras();
					Environment.Exit(0);
				}

				int    muestras = int.Parse(args[0]);
				string archivo  = args[1];
				string config   = args[2];

				// crea el generador
				ETLGenerador generador = new ETLGenerador();
				// configura el generador
				generador.Configurar(config);
				// carga el generador
				generador.Cargar(archivo);

				// configura el archivo de salida
				string salida   = null;
				try
				{
					salida = archivo.Substring(0, archivo.IndexOf(".")) + ".csv";
				}
				catch
				{
					salida = archivo + ".csv";
				}
				// crea el flujo de salida
				StreamWriter writer = new StreamWriter(salida, false, Encoding.Default);

				// lanza el generador
				TimeSpan tiempo = generador.Generar(muestras, writer, false);

				// informa del proceso
				Console.Error.WriteLine();
				Console.Error.WriteLine("INFORME: " + salida);
				Console.Error.WriteLine("---------" + new String('-', salida.Length));
				Console.Error.WriteLine("Tiempo estimado : " + generador.Tiempo(tiempo));
				Console.Error.WriteLine("Filas generadas : " + muestras);
				Console.Error.WriteLine("Filas útiles    : " + (muestras - generador.numFilasDescartadas));
				Console.Error.WriteLine("Filas colisiones: " + generador.numFilasDescartadas);
				Console.Error.WriteLine("---------" + new String('-', salida.Length));
				Console.Error.WriteLine();

				writer.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: {1}", e.GetType(), e.Message);
				Console.WriteLine("{0}", e.StackTrace);
				codigoSalida = 1;
			}

			Environment.Exit(codigoSalida);
		}

		public void Configurar(string archivo)
		{
			// mensaje de estado
			Console.Write("Cargando la configuración ... ");

			configuracion.Cargar(archivo);

			// mensaje de estado
			Console.WriteLine("ok");
		}

		public void Cargar(string archivo)
		{
			// mensaje de estado
			Console.Write("Cargando las definiciones ... ");

			StreamReader  stream = new StreamReader (archivo, Encoding.Default);
			XmlTextReader reader = new XmlTextReader(stream);

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "Tabla")
					{
						while (reader.MoveToNextAttribute())
						{
							if (reader.Name == "sp-campos")
							{
								sepCampos = reader.Value;
								sepCampos = sepCampos.Replace(@"\t", "\t");
							}
							else if (reader.Name == "sp-filas")
							{
								sepFilas = reader.Value;
								sepFilas = sepFilas.Replace(@"\n", "\n");
								sepFilas = sepFilas.Replace(@"\r", "\r");
							}
						}
					}
					else if (reader.Name == "Columnas")
					{
						while (reader.MoveToNextAttribute())
						{
							if (reader.Name == "maxtam")
							{
								tamMax = int.Parse(reader.Value);
							}
							else if (reader.Name == "delim")
							{
								delim = reader.Value;
							}
						}
						break;
					}
				}
			}
			generadores = configuracion.LeerGeneradores(reader);
			// lee las restricciones
			while (reader.Read() && (reader.Name != "Restricciones")) ; // nodo de inicio
			while (reader.Read() && (reader.Name != "Restricciones"))   // nodo de fin
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "candidata")
					{
						ArrayList candidata = new ArrayList();
						while (reader.Read() && (reader.Name != "candidata"))
						{
							if ((reader.Name == "columna") &&
								(reader.NodeType == XmlNodeType.Element))
							{
								reader.Read();
								if (reader.NodeType != XmlNodeType.Text)
								{
									throw new XmlException(
										"se esperaba un nodo de texto (línea " +
										reader.LineNumber + ")");
								}
								bool existeGen = false;
								foreach (ObjGenerador gen in generadores)
								{
									if (gen.Id == reader.Value)
									{
										candidata.Add(reader.Value);
										existeGen = true;
										break;
									}
								}
								if (!existeGen)
								{
									throw new XmlException("no existen columnas con ese nombre " +
										"(línea " + reader.LineNumber + ")");
								}
							}
						}
						if (candidata.Count == 0)
						{
							throw new XmlException("restricción sin columnas");
						}
						candidatas.Add(candidata);
						candidatasGeneradas.Add(new Hashtable());
					}
					else if (reader.Name == "unica")
					{
						ArrayList unica = new ArrayList();
						while (reader.Read() && (reader.Name != "unica"))
						{
							if ((reader.Name == "columna") &&
								(reader.NodeType == XmlNodeType.Element))
							{
								reader.Read();
								if (reader.NodeType != XmlNodeType.Text)
								{
									throw new XmlException(
										"se esperaba un nodo de texto (línea " +
										reader.LineNumber + ")");
								}
								bool existeGen = false;
								foreach (ObjGenerador gen in generadores)
								{
									if (gen.Id == reader.Value)
									{
										unica.Add(reader.Value);
										existeGen = true;
										break;
									}
								}
								if (!existeGen)
								{
									throw new XmlException("no existen columnas con ese nombre " +
										"(línea " + reader.LineNumber + ")");
								}
							}
						}
						if (unica.Count == 0)
						{
							throw new XmlException("restricción sin columnas");
						}
						unicas.Add(unica);
						unicasGeneradas.Add(new Hashtable());
					}
					else if (reader.Name == "no-nulo")
					{
						while (reader.Read() && (reader.Name != "no-nulo"))
						{
							if ((reader.Name == "columna") &&
								(reader.NodeType == XmlNodeType.Element))
							{
								reader.Read();
								if (reader.NodeType != XmlNodeType.Text)
								{
									throw new XmlException(
										"se esperaba un nodo de texto (línea " +
										reader.LineNumber + ")");
								}
								bool existeGen = false;
								foreach (ObjGenerador gen in generadores)
								{
									if (gen.Id == reader.Value)
									{
										noNulos.Add(reader.Value);
										existeGen = true;
										break;
									}
								}
								if (!existeGen)
								{
									throw new XmlException("no existen columnas con ese nombre " +
										"(línea " + reader.LineNumber + ")");
								}
							}
						}
					}
					else
					{
						throw new XmlException("entidad <" + reader.Name + "> no reconocida " +
							reader.LineNumber + ")");
					}
				}
			}

			stream.Close();
			reader.Close();

			// mensaje de estado
			Console.WriteLine("ok");

#if (DEBUG)
			Console.WriteLine("#DEBUG#Cargar(string):");
			foreach (ObjGenerador gen in generadores)
			{
				Console.WriteLine("#DEBUG#\t{0}", gen);
			}
#endif
		}

		// BUG: Para que sumarFilas pueda tomar valor 'true' se debería implementar
		// el registro de las claves ya generadas en el fichero origen
		public TimeSpan Generar(int muestras, StreamWriter writer, bool sumarFilas)
		{
			// contabiliza el tiempo de generación
			DateTime tiempoInicio = DateTime.Now;

			// mensaje de estado
			Console.Write("Generando combinaciones de columnas ... ");

			// inicializa la semilla aleatoria
			Random random = new Random(unchecked((int) DateTime.Now.Ticks));
			// almacena el valor nulo
			string valorNulo = configuracion.GetValorNulo().Substring(
				0, Math.Min(tamMax, configuracion.GetValorNulo().Length));

			// genera las etiquetas de los campos
			string[] intension = new string[generadores.Count];
			for (int i = 0; i < generadores.Count - 1; i++)
			{
				intension[i] = ((ObjGenerador) generadores[i]).Id;
			}
			if (generadores.Count > 0)
			{
				intension[generadores.Count - 1] =
					((ObjGenerador) generadores[generadores.Count - 1]).Id;
			}
			ImprimirCabecera(intension, writer, sumarFilas);

			// genera los campos
			string[] extension = new string [generadores.Count];
			for (uint i = 0; i < muestras; i++)
			{
				for (int j = 0; j < generadores.Count - 1; j++)
				{
					// probabilidad de valor nulo
					double probabilidad = random.NextDouble();
					// si se genera el valor nulo
					if ((probabilidad < configuracion.GetProbabilidadNulo()) &&
						!RestriccionNoNulo(generadores[j]))
					{
						extension[j] = valorNulo;
					}
						// si no se genera el valor nulo
					else
					{
						extension[j] = ((ObjGenerador) generadores[j]).Generar(random).ToString();
					}
				}
				if (generadores.Count > 0)
				{
					// probabilidad de valor nulo
					double probabilidad = random.NextDouble();
					// si se genera el valor nulo
					if ((probabilidad < configuracion.GetProbabilidadNulo()) &&
						!RestriccionNoNulo(generadores[generadores.Count - 1]))
					{
						extension[generadores.Count - 1] = valorNulo;
					}
						// si no se genera el valor nulo
					else
					{
						extension[generadores.Count - 1] = ((ObjGenerador) generadores[generadores.Count - 1]).Generar(random).ToString();
					}
				}
				RegistrarCombinacion(intension, extension, writer, sumarFilas);
			}

			// mensaje de estado
			Console.WriteLine("ok");

			return (DateTime.Now - tiempoInicio);
		}

		private bool RestriccionNoNulo(object generador)
		{
			string columna = ((ObjGenerador) generador).Id;

			bool ca = false;
			foreach (ArrayList candidata in candidatas)
			{
				if (candidata.Contains(columna))
				{
					ca = true;
					break;
				}
			}

			bool nn = noNulos.Contains(columna);

			return (ca || nn);
		}

		private void RegistrarCombinacion(string[] intension, string[] extension,
			StreamWriter writer, bool sumarFilas)
		{
			bool colision = false;

			/*
			 * Chequea colisiones para las claves candidatass
			 */
			for (int c = 0; (c < candidatas.Count) && !colision; c++)
			{
				ArrayList buff = new ArrayList();
				for (int i = 0; i < intension.Length; i++)
				{
					if (((ArrayList) candidatas[c]).Contains(intension[i]))
					{
						buff.Add(extension[i]);
					}
				}

				string codigo = "";
				for (int i = 0; i < buff.Count - 1; i++)
				{
					codigo += delim + buff[i] + delim + sepCampos;
				}
				if (buff.Count > 0)
				{
					codigo += delim + buff[buff.Count - 1] + delim;
				}

				Hashtable candidata = (Hashtable) candidatasGeneradas[c];
				if (candidata[codigo] == null)
				{
					candidata.Add(codigo, true); // por guardar algo
				}
				else
				{
					colision = true; // la fila viola la unicidad
				}
			}

			/*
			 * Chequea colisiones para las claves unicas
			 */
			for (int u = 0; (u < unicas.Count) && !colision; u++)
			{
				ArrayList buff = new ArrayList();
				for (int i = 0; i < intension.Length; i++)
				{
					if (((ArrayList) unicas[u]).Contains(intension[i]))
					{
						buff.Add(extension[i]);
					}
				}
				if (buff.Contains(configuracion.GetValorNulo()))
				{
					break; // los nulos invalidan la unicidad
				}

				string codigo = "";
				for (int i = 0; i < buff.Count - 1; i++)
				{
					codigo += delim + buff[i] + delim + sepCampos;
				}
				if (buff.Count > 0)
				{
					codigo += delim + buff[buff.Count - 1] + delim;
				}

				Hashtable unica = (Hashtable) unicasGeneradas[u];
				if (unica[codigo] == null)
				{
					unica.Add(codigo, true); // por guardar algo
				}
				else
				{
					colision = true; // la fila viola la unicidad
				}
			}

			if (!colision)
			{
				ImprimirFila(extension, writer);
			}
			else
			{
				numFilasDescartadas++;
			}
		}

		private void ImprimirCabecera(string[] columnas, StreamWriter writer, bool sumarFilas)
		{
			// si se habilitan las cabeceras
			if (configuracion.Cabecera)
			{
				if (!sumarFilas)
				{
					// imprime la etiqueta de una marca temporal
					writer.Write(delim + configuracion.MARCA_TEMPORAL + delim + sepCampos);

					// imprime las etiquetas de los campos
					for (int i = 0; i < columnas.Length - 1; i++)
					{
						writer.Write(delim + columnas[i] + delim + sepCampos);
					}
					if (columnas.Length > 0)
					{
						writer.Write(delim + columnas[columnas.Length - 1] + delim);
						writer.Write(sepFilas);
					}
				}
			}
		}

		private DateTime fechaOrigen = new DateTime(0);

		private void ImprimirFila(string[] columnas, StreamWriter writer)
		{
			string fila = delim +
				(long) (DateTime.Now - fechaOrigen).TotalMilliseconds +
				delim + sepCampos;

			for (int i = 0; i < columnas.Length - 1; i++)
			{
				int longitud = Math.Min(tamMax, columnas[i].Length);
				fila += delim + columnas[i].Substring(0, longitud) + delim +
					sepCampos;
			}
			if (columnas.Length > 0)
			{
				int longitud = Math.Min(tamMax, columnas[columnas.Length - 1].Length);
				fila += delim + columnas[columnas.Length - 1].Substring(0, longitud) + delim +
					sepFilas;
			}

			writer.Write(fila);
		}

		private string Tiempo(TimeSpan duracion)
		{
			string tiempo = "";

			int min = (int) (duracion.Minutes);
			if (min == 1)
			{
				tiempo += min + " minuto";
			}
			else if (min > 1)
			{
				tiempo += min + " minutos";
			}

			int seg = (int) (duracion.Seconds);
			if (seg == 1)
			{
				if (tiempo == "")
				{
					tiempo += seg + " segundo";
				}
				else
				{
					tiempo += " " + seg + " segundo";
				}
			}
			else if (seg > 1)
			{
				if (tiempo == "")
				{
					tiempo += seg + " segundos";
				}
				else
				{
					tiempo += " " + seg + " segundos";
				}
			}

			return ((tiempo != "") ? tiempo : "0 segundos");
		}
	}
}
