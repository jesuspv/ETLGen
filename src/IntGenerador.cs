using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de IntGenerador.
	/// </summary>
	public class IntGenerador : ObjGenerador
	{
		private long  min;
		private long  max;
		private ulong paso;

		public IntGenerador() : base()
		{
			entidad    = id = "int";
			this.min   = 0;
			this.max   = 1;
			this.paso  = 1;
		}

		public override object Generar(Random random)
		{
			//int n = (max - min) / (int) paso;
			//int r = random.Next(n + 1);

			long   n = (max - min) / (long) paso;
			double r = n * random.NextDouble();
			return (long) (r * paso) + min;

			//return (long) ((max - min) * random.NextDouble() + min) / (long) paso;

			//return r * paso + min;
		}

		public override ObjGenerador Clonar()
		{
			IntGenerador gen = new IntGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.min  = min;
			gen.max  = max;
			gen.paso = paso;

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id      = null;
			long    min     = -1;
			long    max     = -1;
			long    paso    = -1;
			bool   iniMin  = false;
			bool   iniMax  = false;
			bool   iniPaso = false;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"   : id    = reader.Value; break;
					case "min"  : min   = long.Parse(reader.Value); iniMin  = true; break;
					case "max"  : max   = long.Parse(reader.Value); iniMax  = true; break;
					case "paso" : paso  = long.Parse(reader.Value); iniPaso = true; break;
					default     : ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga el id
			if (id != null)
			{
				this.id = id;
			}
			// carga los límites
			if (iniMin && iniMax)
			{
				if (min > max)
				{
					ErrorValor(entidad, id, "max", max.ToString());
				}
				else
				{
					this.min = min;
					this.max = max;
				}
			}
			else if (iniMin)
			{
				if (min > this.max)
				{
					ErrorValor(entidad, id, "min", min.ToString());
				}
				else
				{
					this.min = min;
				}
			}
			else if (iniMax)
			{
				if (this.min > max)
				{
					ErrorValor(entidad, id, "max", max.ToString());
				}
				else
				{
					this.max = max;
				}
			}
			// carga el paso
			if (iniPaso && (paso >= 0))
			{
				this.paso = (uint) paso;
			}
			else if (iniPaso)
			{
				ErrorValor(entidad, id, "paso", paso.ToString());
			}
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\""     + id     + "\" " +
				"min=\""    + min    + "\" " +
				"max=\""    + max    + "\" " +
				"paso=\""   + paso   + "\" " +
				"/>");
		}
	}
}
