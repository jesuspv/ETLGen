using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de CjtoGenerador.
	/// </summary>
	public abstract class CjtoGenerador : ObjGenerador
	{
		protected ArrayList generadores;

		public CjtoGenerador() : base()
		{
			entidad     = id = "cjto";
			generadores = new ArrayList();
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
			// carga el contenido
			generadores = configuracion.LeerGeneradores(reader);
			// carga el id
			if (id != null)
			{
				this.id = id;
			}
		}

		public override string ToString()
		{
			string gens = "";
			foreach (object obj in generadores)
			{
				gens += obj;
			}

			return ("<" + entidad + " " +
				"id=\"" + id + "\">" +
				gens +
				"</" + entidad + ">");
		}
	}
}
