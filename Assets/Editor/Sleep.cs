using UnityEditor;
using UnityEngine;
using System.Threading;

[InitializeOnLoad]
class MyClass
{
	static MyClass ()
	{
		EditorApplication.update += Update;
	}

	static void Update ()
	{
		Thread.Sleep (1);
	}
}
