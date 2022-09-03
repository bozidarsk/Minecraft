using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public static partial class Noise 
	{
		public static System.Random random;

		public static void Initialize(System.Random random) { Noise.random = random; }
	}
}