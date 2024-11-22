using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Data.DataFactory.DataFactory;

namespace Data.DataFactory
{
	public class DataFactory
	{
		#region _Definitions

		public class SetupParams
		{
			public String FactoryName { get; set; }
			public String AssemblyFilePath { get; set; }
			public Type AssemblySourceType { get; set; }
			public String ConnectionData { get; set; }

			public String ClassName_Setup { get; set; }
			public String ClassName_EntityRepository { get; set; }
			public String ClassName_SystemParamRepository { get; set; }
		}

		#endregion

		#region _Variables

		static Dictionary<String, SetupParams> mAssemblies = new Dictionary<String, SetupParams>();

		#endregion

		#region _Constructor

		public static void Setup_AddAssembly(SetupParams Params)
		{
			if (!mAssemblies.ContainsKey(Params.FactoryName))
			{ mAssemblies.Add(Params.FactoryName, Params); }
			else
			{ mAssemblies[Params.FactoryName] = Params; }
		}

		#endregion

		#region _Methods

		public static Interface_EntityRepository<T_Entity> Create_EntityRepository<T_Entity>(String FactoryName)
			where T_Entity : class, new()
		{
			var DataFactory_Instanced = DataFactory.Create_DataFactory(FactoryName);
			return DataFactory_Instanced.Create_EntityRepository<T_Entity>();
		}

		public static Interface_SystemParamRepository Create_SystemParamRepository(String FactoryName)
		{
			var DataFactory_Instanced = DataFactory.Create_DataFactory(FactoryName);
			return DataFactory_Instanced.Create_SystemParamRepository();
		}

		public static Interface_EntityQuery<T_Entity> Create_EntityQuery<T_Entity>(String FactoryName, String QueryName)
			where T_Entity : class, new()
		{
			var DataFactory_Instanced = DataFactory.Create_DataFactory(FactoryName);
			return DataFactory_Instanced.Create_EntityQuery<T_Entity>(QueryName);
		}

		public static DataFactory_Instance Create_DataFactory(String FactoryName)
		{
			var SetupParams = DataFactory.mAssemblies[FactoryName];
			DataFactory_Instance Data = new DataFactory_Instance(SetupParams);
			return Data;
		}

		#endregion
	}

	public class DataFactory_Instance
	{
		#region _Variables

		SetupParams mSetupParams;
		Assembly mAssembly;
		Interface_Setup mInstanced_Setup;

		#endregion

		#region _Constructor

		public DataFactory_Instance(SetupParams SetupParams)
		{
			this.mSetupParams = SetupParams;

			Assembly AssemblyObj = null;

			if (!String.IsNullOrEmpty(this.mSetupParams.AssemblyFilePath))
			{ AssemblyObj = Assembly.LoadFrom(this.mSetupParams.AssemblyFilePath); }
			else if (this.mSetupParams.AssemblySourceType != null)
			{ AssemblyObj = Assembly.GetAssembly(this.mSetupParams.AssemblySourceType); }

			var Instanced_Setup = this.Invoke_Setup(AssemblyObj);
			this.mInstanced_Setup = Instanced_Setup;
			this.mAssembly = AssemblyObj;
		}

		#endregion

		#region _Methods.Instance

		public Interface_EntityRepository<T_Entity> Create_EntityRepository<T_Entity>()
			where T_Entity : class, new()
		{
			Assembly AssemblyObj = this.mAssembly;
			String AssemblyName = AssemblyObj.GetName().Name;
			String EntityRepositoryName = this.mSetupParams.ClassName_EntityRepository;
			String EntityRepository_ClassName = String.Format("{0}.{1}", AssemblyName, EntityRepositoryName);

			Type EntityRepository_ClassType =
				AssemblyObj
					.GetTypes()
					.Where(O =>
						O.IsGenericType == true
						&& O.FullName.Contains(EntityRepository_ClassName)
						)
					.FirstOrDefault();

			var EntityRepository_ClassType_Generic = EntityRepository_ClassType.MakeGenericType(typeof(T_Entity));
			var Instanced = Activator.CreateInstance(EntityRepository_ClassType_Generic, new Object[] { this.mInstanced_Setup });
			return (Instanced as Interface_EntityRepository<T_Entity>);
		}

		public Interface_SystemParamRepository Create_SystemParamRepository()
		{
			Assembly AssemblyObj = this.mAssembly;
			String AssemblyName = AssemblyObj.GetName().Name;
			String SystemParamRepositoryName = this.mSetupParams.ClassName_SystemParamRepository;
			String SystemRepository_ClassName = String.Format("{0}.{1}", AssemblyName, SystemParamRepositoryName);

			Type EntityRepository_ClassType = AssemblyObj.GetType(SystemRepository_ClassName);
			var Instanced = Activator.CreateInstance(EntityRepository_ClassType, new Object[] { this.mInstanced_Setup });
			return (Instanced as Interface_SystemParamRepository);
		}

		public Interface_EntityQuery<T_Entity> Create_EntityQuery<T_Entity>(String QueryName)
			where T_Entity : class, new()
		{
			Assembly AssemblyObj = this.mAssembly;
			String AssemblyName = AssemblyObj.GetName().Name;
			String QueryClassName = String.Format("{0}.{1}", AssemblyName, QueryName);
			Type QueryClassType = AssemblyObj.GetType(QueryClassName);
			Boolean Is_Generic = false;
			if (QueryClassType == null)
			{
				Is_Generic = true;

				QueryClassType =
					AssemblyObj
						.GetTypes()
						.Where(O =>
							O.IsGenericType == true
							&& O.FullName.Contains(QueryClassName)
							)
						.FirstOrDefault();
			}

			Object Instanced = null;

			var Constructor_WithSetup = QueryClassType.GetConstructor(new Type[] { typeof(Interface_Setup) });
			if (Constructor_WithSetup != null)
			{
				if (Is_Generic)
				{
					var QueryClassType_Generic = QueryClassType.MakeGenericType(typeof(T_Entity));
					Instanced = Activator.CreateInstance(QueryClassType_Generic, new Object[] { this.mInstanced_Setup });
				}
				else
				{
					Instanced = Activator.CreateInstance(QueryClassType, new Object[] { this.mInstanced_Setup });
				}
			}
			else
			{
				if (Is_Generic)
				{
					var QueryClassType_Generic = QueryClassType.MakeGenericType(typeof(T_Entity));
					Instanced = Activator.CreateInstance(QueryClassType_Generic);
				}
				else
				{ Instanced = Activator.CreateInstance(QueryClassType); }
			}

			return (Instanced as Interface_EntityQuery<T_Entity>);
		}

		Interface_Setup Invoke_Setup(Assembly AssemblyObj)
		{
			String AssemblyName = AssemblyObj.GetName().Name;
			String ClassName_Setup = this.mSetupParams.ClassName_Setup;
			ClassName_Setup = String.Format("{0}.{1}", AssemblyName, ClassName_Setup);

			Type ClassType_Setup = AssemblyObj.GetType(ClassName_Setup);
			var Instanced = (Activator.CreateInstance(ClassType_Setup) as Interface_Setup);

			Instanced.Setup_Connection(this.mSetupParams.ConnectionData);

			return Instanced;
		}

		#endregion
	}
}
