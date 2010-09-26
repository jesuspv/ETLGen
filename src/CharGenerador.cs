using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de CharGenerador.
	/// </summary>
	public class CharGenerador : ObjGenerador
	{
		private string tipo;
		private int min;
		private int max;

		public CharGenerador() : base()
		{
			entidad = id = "char";

			// mayúsculas y minúsculas
			tipo = "";
			// existe una ordenación secuencial A-Z y a-Z
			min = Math.Min((int) 'a', (int) 'A');
			max = Math.Max((int) 'z', (int) 'Z') + 1;
		}

		public override object Generar(Random random)
		{
			return (char) random.Next(min, max);
		}

		public override ObjGenerador Clonar()
		{
			CharGenerador gen = new CharGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.tipo = tipo;
			gen.min  = min;
			gen.max  = max;

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id    = null;
			string tipo  = null;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"   : id    = reader.Value; break;
					case "tipo" : tipo  = reader.Value; break;
					default     : ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga el id
			if (id != null) this.id = id;
			// carga el tipo
			if (tipo != null)
			{
				switch (tipo)
				{
					case "M": // Mayúsculas
						min = (int) 'A';
						max = (int) 'Z' + 1;
						break;
					case "m": // minúsculas
						min = (int) 'a';
						max = (int) 'z' + 1;
						break;
					default:
						ErrorValor(entidad, id, "tipo", tipo);
						break;
				}
				this.tipo = tipo;
			}
			else
			{
				// existe una ordenación secuencial A-Z y a-Z
				min = Math.Min((int) 'a', (int) 'A');
				max = Math.Max((int) 'z', (int) 'Z') + 1;
			}
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\""  + id + "\" " +
				((tipo != "") ? ("tipo=\"" + tipo + "\" ") : "") +
				"/>");
		}
	}
}
