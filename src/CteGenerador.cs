using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de CteGenerador.
	/// </summary>
	public class CteGenerador : ObjGenerador
	{
		private string constante;

		public CteGenerador() : base()
		{
			entidad   = id = "cte";
			constante = "";
		}

		public override object Generar(Random random)
		{
			return constante;
		}

		public override ObjGenerador Clonar()
		{
			CteGenerador gen = new CteGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.constante = constante;

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

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id"   : id    = reader.Value; break;
					default     : ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga la constante
			if (reader.Read() && reader.NodeType == XmlNodeType.Text)
			{
				constante = reader.Value;
			}
			else
			{
				throw new XmlException(GetType() +
					": se esperaba un nodo de texto (línea " + reader.LineNumber + ")");
			}
			// @TODO Solucionar
			if (!reader.Read() || reader.NodeType != XmlNodeType.EndElement)
			{
				throw new XmlException("se esperaba la etiqueta de cierre");
			}
			// carga el id
			if (id != null) this.id = id;
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\"" + id + "\">" +
				constante +
				"</" + entidad + ">");
		}
	}
}