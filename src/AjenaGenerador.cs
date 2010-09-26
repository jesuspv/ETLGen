using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de AjenaGenerador.
	/// </summary>
	public class AjenaGenerador : ObjGenerador
	{
		// @TODO
		private static readonly string TABLA_EXT  = ".csv";
		private static readonly string TABLA_PATH = "data/";

		private string tabla;
		private char   delimColum; /* Ojo!         */
		private char   spColum;    /* Deberían ser */
		private char   spFilas;    /* distintos    */
		private ArrayList especColumnas;
		private bool      especNumeros;
		private Hashtable filas;

		public AjenaGenerador() : base()
		{
			entidad = id = "ajena";

			tabla      = "";
			delimColum = '"';
			spColum    = ';';
			spFilas    = '\n';
			filas      = new Hashtable();
			especColumnas = new ArrayList();
			especNumeros  = true;
		}

		public override object Generar(Random random)
		{
			return filas[random.Next(filas.Count)];
		}

		public override ObjGenerador Clonar()
		{
			AjenaGenerador gen = new AjenaGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			tabla      = gen.tabla;
			delimColum = gen.delimColum;
			spColum    = gen.spColum;
			spFilas    = gen.spFilas;
			filas      = (Hashtable) gen.filas.Clone();
			especColumnas = (ArrayList) gen.especColumnas.Clone();
			especNumeros  = gen.especNumeros;

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id         = null;
			string tabla      = null;
			string delimColum = null;
			string spColum    = null;
			string spFilas    = null;
			ArrayList nums = new ArrayList();
			ArrayList noms = new ArrayList();

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"         : id         = reader.Value; break;
					case "tabla"      : tabla      = reader.Value; break;
					case "delim-colum": delimColum = reader.Value; break;
					case "sp-colum"   : spColum    = reader.Value; break;
					case "sp-filas"   : spFilas    = reader.Value; break;
					case "nom-colum":
						noms.Add(reader.Value);
						break;
					case "num-colum":
						if (int.Parse(reader.Value) <= 0)
						{
							ErrorValor(entidad, id, "num-colum", reader.Value);
						}
						nums.Add(int.Parse(reader.Value).ToString());
						break;
					default: ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga la especificación de las columnas
			if ((nums.Count == 0) && (noms.Count == 0))
			{
				ErrorValor(entidad, id, "nombre-columnas", "");
			}
			else if ((nums.Count != 0) && (noms.Count != 0))
			{
				ErrorValor(entidad, id, "nombre-columnas", "");
			}
			else
			{
				if (nums.Count != 0)
				{
					especColumnas = nums;
					especNumeros  = true;
				}
				else // noms.Count != 0
				{
					especColumnas = noms;
					especNumeros  = false;
				}
			}
			// descompone la clave ajena en las columnas necesarias
			Descomponer();
			// carga el id
			if (id != null) this.id = id;
			// carga el delimitador de columnas
			if (delimColum != null)
			{
				delimColum = delimColum;
				if (delimColum.Length > 1)
				{
					ErrorValor(entidad, id, "delim-colum", delimColum);
				}
				else
				{
					this.delimColum = delimColum[0];
				}
			}
			// carga el separador de columnas
			if (spColum != null)
			{
				spColum = spColum.Replace(@"\t", "\t");
				if (spColum.Length > 1)
				{
					ErrorValor(entidad, id, "sp-colum", spColum);
				}
				else
				{
					this.spColum = spColum[0];
				}
			}
			// carga el separador de filas
			if (spFilas != null)
			{
				spFilas = spFilas.Replace(@"\n", "\n");
				spFilas = spFilas.Replace(@"\r", "\r");
				if (spFilas.Length > 1)
				{
					ErrorValor(entidad, id, "sp-filas", spFilas);
				}
				else
				{
					this.spFilas = spFilas[0];
				}
			}
			// carga la tabla
			if (tabla != null)
			{
				CargarFilas(TABLA_PATH + tabla + TABLA_EXT);
				this.tabla = tabla;
			}
			else
			{
				ErrorValor(entidad, id, "tabla", "");
			}
		}

		private void CargarFilas(string archivo)
		{
			// solicita recursos IMPORTANTE LA CODIFICACIÓN
			StreamReader stream = new StreamReader(archivo, Encoding.Default);

			/*
			 * Recoge la primera línea si contiene la especificación
			 */
			if (!especNumeros)
			{
				bool   encontrado  = false;
				bool   colAbierta1 = false;
				StringBuilder colBuff1 = new StringBuilder(1024);
				int    colNumero1  = 0;
				int    input1;
				while ((input1 = stream.Read()) != -1 && input1 != spFilas)
				{
					char caracter = (char) input1;

					if (caracter == delimColum)
					{
						if (!colAbierta1)
						{
							colAbierta1 = true;
							colNumero1++;
						}
						else
						{
							colAbierta1 = false;
							if (especColumnas.Contains(colBuff1.ToString()))
							{
								//especColumnas.Clear();
								int idx = especColumnas.IndexOf(colBuff1.ToString());
								especColumnas[idx] = colNumero1.ToString();
								//especColumnas.Add(colNumero1);
								encontrado = true;
							}
							colBuff1.Remove(0, colBuff1.Length);//
						}
					}
					else if (caracter == spColum)
					{
						if (colAbierta1) throw new ArgumentException(archivo +
											": columna sin completar");
					}
					else
					{
						colBuff1.Append(caracter);
					}
				}

				if (!encontrado) throw new ArgumentException(archivo +
					": especificación del nombre de la columna no válida");
			}

			/*
			 * Recoge los valores a referenciar
			 */
			bool   colAbierta = false;
			StringBuilder colBuff = new StringBuilder(1024);
			int    colNumero  = 0;
			ArrayList cols    = new ArrayList();
			for (int i = 0; i < especColumnas.Count; i++) cols.Add(0);
			int    input;
			while ((input = stream.Read()) != -1)
			{
				char caracter = (char) input;

				if (caracter == delimColum)
				{
					if (!colAbierta)
					{
						colAbierta = true;
						colNumero++;
					}
					else
					{
						colAbierta = false;
						if (especColumnas.Contains(colNumero.ToString()))
						{
							int idx = especColumnas.IndexOf(colNumero.ToString());
							cols[idx] = colBuff.ToString();
						}
						colBuff.Remove(0, colBuff.Length);
					}
				}
				else if (caracter == spColum)
				{
					if (colAbierta) throw new ArgumentException(archivo +
						": columna sin completar");
				}
				else if (caracter == spFilas)
				{
					if (colAbierta)
					{
						throw new ArgumentException(archivo +
							": fila con columna sin completar");
					}
					colNumero = 0;

					string columnas = "";
					columnas += cols[0];
					for (int i = 1; i < cols.Count; i++)
					{
						columnas += delimColum.ToString() + spColum.ToString() + delimColum + cols[i];
					}
					filas.Add(filas.Count, columnas);
				}
				else
				{
					colBuff.Append(caracter);
				}
			}

			stream.Close();
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\""         + id         + "\" " +
				"tabla=\""      + tabla      + "\" " +
				"delimColum=\"" + delimColum + "\" " +
				"spColum=\""    + spColum    + "\" " +
				"spFilas=\""    + spFilas    + "\" " +
				"/>");
		}

		/// <summary>
		/// Descompone la especificación de la clave ajena.
		/// </summary>
		private void Descomponer()
		{
			string especificacion = especColumnas[0].ToString();
			StringReader reader = new StringReader(especificacion);

			// limpia la especificación
			especColumnas.Clear();

			StringBuilder colBuff = new StringBuilder(1024);
			int    input;
			while ((input = reader.Read()) != -1 && input != spFilas)
			{
				char caracter = (char) input;

				if (caracter == spColum)
				{
					especColumnas.Add(colBuff.ToString());
					colBuff.Remove(0, colBuff.Length);
				}
				else
				{
					colBuff.Append(caracter);
				}
			}
			especColumnas.Add(colBuff.ToString());
			colBuff.Remove(0, colBuff.Length);
		}
	}
}
