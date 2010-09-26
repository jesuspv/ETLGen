using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de DisGenerador.
	/// </summary>
	public class DisGenerador : CjtoGenerador
	{
		public DisGenerador() : base()
		{
			entidad = id = "dis";
		}

		public override object Generar(Random random)
		{
			int          idx  = random.Next(generadores.Count);
			ObjGenerador gen = (ObjGenerador) generadores[idx];

			return gen.Generar(random);
		}

		public override ObjGenerador Clonar()
		{
			DisGenerador gen = new DisGenerador();
			gen.entidad = entidad;
			gen.id      = gen.id;

			gen.generadores = new ArrayList(generadores);

			return gen;
		}
	}
}
