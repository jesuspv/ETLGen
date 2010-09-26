using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de ConGenerador.
	/// </summary>
	public class ConGenerador : CjtoGenerador
	{
		public ConGenerador() : base()
		{
			entidad = id = "con";
		}

		public override object Generar(Random random)
		{
			string valores = "";
			foreach (ObjGenerador gen in generadores)
			{
				valores += gen.Generar(random);
			}

			return valores;
		}

		public override ObjGenerador Clonar()
		{
			ConGenerador gen = new ConGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.generadores = new ArrayList(generadores);

			return gen;
		}
	}
}
