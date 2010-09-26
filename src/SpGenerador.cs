using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de SpGenerador.
	/// </summary>
	public class SpGenerador : ObjGenerador
	{
		private string valor;

		public SpGenerador() : base()
		{
			entidad = id = "sp";
			valor   = " ";
		}

		public override object Generar(Random random)
		{
			return valor;
		}

		public override ObjGenerador Clonar()
		{
			SpGenerador gen = new SpGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.valor = valor;

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
			string val   = null;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"   : id    = reader.Value; break;
					case "val"  : val   = reader.Value; break;
					default     : ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga el id
			if (id != null) this.id = id;
			// carga el valor
			if (val != null) this.valor = val;
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\"" + id + "\" " +
				((valor != "") ? ("valor=\"" + valor + "\" ") : "") +
				"/>");
		}
	}
}
