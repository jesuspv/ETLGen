using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Guarda la configuración de la aplicación.
	/// </summary>
	public class ETLConfig
	{
		/// <summary>
		/// Cabecera para las columnas de marca temporal.
		/// </summary>
		public readonly string MARCA_TEMPORAL = "MARCA_TEMPORAL";

		/// <summary>
		/// Asociaciones de elementos XML con generadores.
		/// </summary>
		/// <remarks>
		/// Las claves son los nombres de los elementos y los valores
		/// son objetos de tipo <code>etlgen.ObjGenerador</code>.
		/// </remarks>
		private Hashtable asociaciones;

		/// <summary>
		/// Valor nulo para los valores generados para las columnas.
		/// </summary>
		private string valorNulo;

		/// <summary>
		/// Probabilidad de que los generadores generen un valor nulo.
		/// </summary>
		/// <remarks>
		/// El valor de probabilidad viene comprendido en el rango [0,1].
		/// </remarks>
		private double probNulo;

		/// <summary>
		/// Rutas de tablas.
		/// </summary>
		private ArrayList rutas;

		/// <summary>
		/// Habilitación de la impresión de cabeceras.
		/// </summary>
		private bool cabecera;

		/// <summary>
		/// Construye un objeto para almacenar la configuración.
		/// </summary>
		public ETLConfig()
		{
			asociaciones = new Hashtable();
			valorNulo    = "NULL";
			probNulo     = 0.0;
			rutas        = new ArrayList();
			cabecera     = false;
		}

		/// <summary>
		/// Carga la configuración ETL desde un archivo XML de configuración.
		/// </summary>
		/// <param name="archivo">Nombre del archivo de configuración.</param>
		/// <exception cref="System.ArgumentException"><see cref="System.IO.StreamReader" /></exception> 
		/// <exception cref="System.ArgumentNullException"><see cref="System.IO.StreamReader" /></exception>
		/// <exception cref="System.IO.FileNotFoundException"><see cref="System.IO.StreamReader" /></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"><see cref="System.IO.StreamReader" /></exception>
		/// <exception cref="System.IO.IOException"><see cref="System.IO.StreamReader" /></exception>
		/// <exception cref="System.Xml.XmlException"><see cref="System.Xml.XmlTextReader" /></exception>
		/// <exception cref="System.FormatException"><see cref="System.Double" /></exception>
		/// <exception cref="System.OverflowException"><see cref="System.Double" /></exception>
		/// <exception cref="System.IO.FileNotFoundException"><see cref="System.Reflection.Assembly" /></exception>
		/// <exception cref="System.Security.SecurityException"><see cref="System.Reflection.Assembly" /></exception>
		/// <exception cref="System.ArgumentException"><see cref="System.Reflection.Assembly" /></exception>
		/// <exception cref="System.MissingMethodException"><see cref="System.Reflection.Assembly" /></exception>
		/// <exception cref="System.NotSupportedException"><see cref="System.Reflection.Assembly" /></exception>
		/// <exception cref="System.InvalidCastException">
		/// </exception><paramref name="archivo" /> contiene un generador que no hereda de <code>ObjGenerador</code>.
		/// <exception cref="System.ArgumentException">
		/// </exception><paramref name="archivo" /> contiene una asociación duplicada para el mismo elemento.
		/// </exception>
		public void Cargar(string archivo)
		{
			// solicita recursos
			StreamReader  stream = new StreamReader (archivo, Encoding.UTF7);
			XmlTextReader reader = new XmlTextReader(stream);

			while (reader.Read())
			{
				// carga los atributos de <Generadores>
				if ((reader.NodeType == XmlNodeType.Element) &&
					(reader.Name == "Generadores"))
				{
					// carga los atributos
					string valor_nulo = null;
					string prob_nulo  = null;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
							case "valor-nulo": valor_nulo = reader.Value; break;
							case "prob-nulo" : prob_nulo  = reader.Value; break;
							default          : throw new XmlException(GetType() +
												   ": atributo no reconocido (línea " + reader.LineNumber + ")");
						}
					}
					// carga el valor nulo
					if (valor_nulo != null)
					{
						this.valorNulo = valor_nulo;
					}
					// carga la probabilidad de nulo
					if (prob_nulo != null)
					{
						NumberStyles     estilo  = NumberStyles.Float;
						NumberFormatInfo formato = new NumberFormatInfo();
						formato.NumberDecimalSeparator = ".";
						probNulo = double.Parse(prob_nulo, estilo, formato);
					}
				}
				// lee un elemento de <Generadores>
				else if ((reader.NodeType == XmlNodeType.Element) &&
					(reader.Name == "asociar"))
				{
					// carga los atributos
					string elemento  = null;
					string generador = null;
					string ruta      = null;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
							case "elemento" : elemento  = reader.Value; break;
							case "generador": generador = reader.Value; break;
							case "ruta"     : ruta      = reader.Value; break;
							default         : throw new XmlException(GetType() +
												  ": atributo no reconocido (línea " + reader.LineNumber + ")");
						}
					}
					if ((elemento == null) || (generador == null) ||(ruta == null) || (elemento == ""))
					{
						throw new XmlException(GetType() + ": elemento <asociar> no válido");
					}

					// almacena la asociación
					Assembly assembly = Assembly.LoadFile(ruta);
					ObjGenerador gen = (ObjGenerador) assembly.CreateInstance(generador);
					asociaciones.Add(elemento, gen);
				}
				// lee un elemento de <Importaciones>
				else if ((reader.NodeType == XmlNodeType.Element) &&
					(reader.Name == "ruta"))
				{
					// @TODO Documentar
					reader.Read(); // se mueve al texto contenido
					Hashtable gens = Importar(reader.Value);
					foreach (string elemento in gens.Keys)
					{
						asociaciones.Add(elemento, gens[elemento]);
					}
				}
				// lee un elemento de <Parametros>
				else if ((reader.NodeType == XmlNodeType.Element) &&
					(reader.Name == "cabecera"))
				{
					// registra la habilitación de la cabecera
					cabecera = true;
				}
			}

			// libera los recursos
			reader.Close();
			stream.Close();
		}

		
		/// @TODO
		public Hashtable Importar(string directorio)
		{
			// valor de retorno
			Hashtable buff = new Hashtable();

			string[] archivos = Directory.GetFiles(directorio, "*.xml");
			foreach (string archivo in archivos)
			{
				// solicita recursos
				StreamReader  stream = new StreamReader (archivo, Encoding.Default);
				XmlTextReader reader = new XmlTextReader(stream);

				// lee los generadores
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
					if (asociaciones.Contains(reader.Name))
					{
						ObjGenerador gen = GetGenerador(reader.Name);
						gen.Cargar(reader, this);
						asociaciones.Add(gen.Id, gen);
					}
				}
/*
				// recupera la definición
				ArrayList generadores = LeerGeneradores(reader);
				foreach (ObjGenerador gen in generadores)
				{
					buff.Add(gen.Id, gen);
				}
*/
				// libera los recursos
				reader.Close();
				stream.Close();
			}

			return buff;
		}

		/// <summary>
		/// Descarga la configuración ETL.
		/// </summary>
		public void Descargar()
		{
			asociaciones = new Hashtable();
			valorNulo    = "NULL;";
			probNulo     = 0.0;
			rutas        = new ArrayList();
			cabecera     = false;
		}

		/// <summary>
		/// Recupera una copia de un generador.
		/// </summary>
		/// <param name="elemento">Nombre del elemento que tiene asociado el generador.</param>
		/// <returns>
		///		El generador asociado a la clave <paramref name="elemento" />.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="elemento" /> es <code>null</code>.
		/// </exception>
		/// <exception cref="System.NullReferenceException">
		/// No hay registrado un generador para <paramref name="elemento" />.
		/// </exception>
		public ObjGenerador GetGenerador(string elemento)
		{
			return ((ObjGenerador) asociaciones[elemento]).Clonar();
		}

		/// <summary>
		/// Obtiene el valor nulo generable para las columnas.
		/// </summary>
		/// <returns>El valor nulo.</returns>
		public string GetValorNulo()
		{
			return valorNulo;
		}

		/// <summary>
		/// Obtiene la probabilidad de que una columna se genere con valor nulo.
		/// </summary>
		/// <returns>
		/// Valor de probabilidad entre [0,1] siendo 1 la máxima probabilidad de nulo.
		/// </returns>
		public double GetProbabilidadNulo()
		{
			return probNulo;
		}

		/// @TODO
		public ArrayList LeerGeneradores(XmlTextReader reader)
		{
			// valor de retorno
			ArrayList buff = new ArrayList();

			// recoge los elementos asociados a los generadores registrados
			ArrayList elementos = new ArrayList();
			elementos.AddRange(asociaciones.Keys);

			// lee los generadores
			while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement))
			{
				if (elementos.Contains(reader.Name))
				{
					ObjGenerador gen = GetGenerador(reader.Name);
					gen.Cargar(reader, this);
					buff.Add(gen);
				}
			}

			return buff;
		}

		/// <summary>
		/// Habilitación de la impresión de cabeceras.
		/// </summary>
		public bool Cabecera
		{
			get
			{
				return cabecera;
			}
		}
	}
}
