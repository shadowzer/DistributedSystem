﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node
{
	public static class Storage
	{
		public static string MasterIP;
		public static string FilePath;
		public static HashSet<string> Replicas = new HashSet<string>();
	}
}