using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de FechaGenerador.
	/// </summary>
	public class FechaGenerador : ObjGenerador
	{
		private string file;

		private Hashtable fechas;

		public FechaGenerador() : base()
		{
			entidad = id = "da_smart_key";

			fechas = new Hashtable();
		}

		public override object Generar(Random random)
		{
			return fechas[random.Next(fechas.Count)];
		}

		public override ObjGenerador Clonar()
		{
			FechaGenerador gen = new FechaGenerador();
			gen.entidad = entidad;
			gen.id      = id;
			gen.fechas = (Hashtable) fechas.Clone();

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id   = null;
			string file = null;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"  : id  = reader.Value;  break;
					case "file": file = reader.Value; break;
					default   : ErrorAtributo(entidad,
									reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga el id
			if (id != null) this.id = id;
			// carga el archivo
			if (file != null)
			{
				this.file = file;
			}
			else
			{
				ErrorValor(entidad, id, "file", "");
			}

			CargarFechas();
		}

		void CargarFechas()
		{
			StreamReader stream = null;

			try
			{
				stream = new StreamReader(file);
			}
			catch
			{
				throw new Exception("El archivo " + file + " no se puede abrir.");
			}

			string linea = null;
			while ((linea = stream.ReadLine()) != null)
			{
				try
				{
					long fecha = long.Parse(linea);
					fechas.Add(fechas.Count, fecha);
				}
				catch(FormatException)
				{
					; // no hace nada
				}
			}
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\""   + id   + "\" " +
				"file=\"" + file + "\" ");
		}
	}
}
