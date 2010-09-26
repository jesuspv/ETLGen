using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de RefGenerador.
	/// </summary>
	public class RefGenerador : ObjGenerador
	{
		private string gen;
		private ObjGenerador generador;

		public RefGenerador() : base()
		{
			entidad = id = "ref";
			gen = "";
			generador = null;
		}

		public override object Generar(Random random)
		{
			return generador.Generar(random);
		}

		public override ObjGenerador Clonar()
		{
			RefGenerador gen = new RefGenerador();
			gen.entidad = entidad;
			gen.id      = id;

			gen.gen       = this.gen;
			gen.generador = generador;

			return gen;
		}

		public override void Cargar(XmlTextReader reader, ETLConfig configuracion)
		{
			if ((reader.NodeType != XmlNodeType.Element) ||
				(reader.Name != entidad))
			{
				ErrorEntidad(entidad);
			}

			string id  = null;
			string gen = null;

			// carga los atributos
			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "id" : id  = reader.Value; break;
					case "gen": gen = reader.Value; break;
					default   : ErrorAtributo(entidad,
						reader.Name, reader.Value, reader.LineNumber);
						break;
				}
			}
			// carga el id
			if (id != null) this.id = id;
			// carga la referencia
			if (gen != null)
			{
				this.gen = gen;
			}
			else
			{
				ErrorValor(entidad, id, "gen", "");
			}
			// carga el generador referenciado
			generador = configuracion.GetGenerador(gen);
		}

		public override string ToString()
		{
			return ("<" + entidad + " " +
				"id=\"" + id + "\" " +
				((gen != "") ? ("gen=\"" + gen + "\" ") : "") +
				"/>");
		}
	}
}
