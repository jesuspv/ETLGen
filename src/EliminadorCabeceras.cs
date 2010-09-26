using System;

namespace etlgen
{
	/// <summary>
	/// Elimina la primera linea de la entrada est�ndar.
	/// </summary>
	public class EliminadorCabeceras
	{
		public EliminadorCabeceras()
		{
			// no hace nada con la primera l�nea
			string linea = Console.ReadLine();

			while ((linea = Console.ReadLine()) != null)
			{
				Console.WriteLine(linea);
			}
		}
	}
}
