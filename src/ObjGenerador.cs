using System;
using System.Collections;
using System.Xml;

namespace etlgen
{
	/// <summary>
	/// Descripción breve de ObjGenerador.
	/// </summary>
	public abstract class ObjGenerador
	{
		protected string entidad;
		protected string id;

		public ObjGenerador ()
		{
			entidad = id = "obj";
			id      = "";
		}

		public abstract object Generar(Random random);
		public abstract ObjGenerador Clonar();
		public abstract void Cargar(XmlTextReader reader, ETLConfig configuracion);

		public string Entidad
		{
			get
			{
				return entidad;
			}
		}

		public string Id
		{
			get
			{
				return id;
			}

			set
			{
				if (value != null)
				{
					id = value;
				}
				else
				{
					throw new ArgumentNullException();
				}
			}
		}

		protected void ErrorEntidad(string entidad)
		{
			throw new XmlException(GetType() + ": se esperaba una entidad <" + entidad + ">");
		}

		protected void ErrorAtributo(string entidad, string atributo, string valor,
			int linea)
		{
			throw new XmlException(GetType() + ": entidad <" + entidad + "> " +
				"con atributo '" + atributo + "=\"" + valor +
				"\"' no reconocido (línea " + linea + ")");
		}

		protected void ErrorValor(string entidad, string id, string atributo, string valor)
		{
			throw new XmlException(GetType() + ": entidad <" + entidad + ">" +
				((id != null) ? " de clase \"" + id + "\"": "") +
				" con atributo '" + atributo + "=\"" + valor + "\"' de valor incorrecto");
		}
	}
}
