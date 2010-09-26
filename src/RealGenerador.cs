using System;
using System.Collections;
using System.Globalization;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de RealGenerador.
	/// </summary>
	public class RealGenerador : ObjGenerador
	{
		private double min;
		private double max;
		private double paso;
		private uint   escala;

		private NumberStyles     estilo;
		private NumberFormatInfo formatoEntrada;
		private NumberFormatInfo formatoSalida;

		public RealGenerador() : base()
		{
			entidad = id = "real";
			min    = 0.0;
			max    = 1.0;
			paso   = 0.01;
			escala = 2;

			estilo  = NumberStyles.Float;
			formatoEntrada = new NumberFormatInfo();
			formatoEntrada.NumberDecimalSeparator = ".";
			// @TODO: formatoEntrada.NumberDecimalDigits    = (int) escala;
			// IMPORTANTE PARA EVITAR EL ERROR ORA-01722
			formatoSalida = new NumberFormatInfo();
			formatoSalida.NumberDecimalSeparator = ",";
		}

		public override object Generar(Random random)
		{
			double n = (max - min) / paso;
			double r = Math.Round(random.NextDouble() * n, 0);
			double v = (r * paso) + min;

			return v.ToString("F" + escala, formatoSalida);
		}

		public override ObjGenerador Clonar()
		{
			RealGenerador gen = new RealGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.min    = min;
			gen.max    = max;
			gen.paso   = paso;
			gen.escala = escala;

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id        = null;
			double min       = 0.0;
			double max       = 0.0;
			double paso      = -1.0;
			int    escala    = -1;
			bool   iniMin    = false;
			bool   iniMax    = false;
			bool   iniPaso   = false;
			bool   iniEscala = false;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"    : id     = reader.Value; break;
					case "min"   : min    = double.Parse(reader.Value, estilo, formatoEntrada); iniMin    = true; break;
					case "max"   : max    = double.Parse(reader.Value, estilo, formatoEntrada); iniMax    = true; break;
					case "paso"  : paso   = double.Parse(reader.Value, estilo, formatoEntrada); iniPaso   = true; break;
					case "escala": escala = int   .Parse(reader.Value, estilo, formatoEntrada); iniEscala = true; break;
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
					ErrorValor(entidad, id, "max", max.ToString("F" + escala, formatoEntrada));
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
					ErrorValor(entidad, id, "min", min.ToString("F" + escala, formatoEntrada));
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
					ErrorValor(entidad, id, "max", max.ToString("F" + escala, formatoEntrada));
				}
				else
				{
					this.max = max;
				}
			}
			// carga el paso
			if (iniPaso && (paso >= 0.0))
			{
				this.paso = paso;
			}
			else if (iniPaso)
			{
				ErrorValor(entidad, id, "paso", paso.ToString("F" + escala, formatoEntrada));
			}
			// carga la precision
			// carga el paso
			if (iniEscala && (escala >= 0))
			{
				this.escala = (uint) escala;
			}
			else if (iniEscala)
			{
				ErrorValor(entidad, id, "escala", escala.ToString());
			}
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\""     + id + "\" " +
				"min=\""    + min.ToString(formatoEntrada)  + "\" " +
				"max=\""    + max.ToString(formatoEntrada)  + "\" " +
				"paso=\""   + paso.ToString(formatoEntrada) + "\" " +
				"escala=\"" + escala + "\" " +
				"/>");
		}
	}
}