using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Editor;

public static partial class CreateModelWithScaleAndMirror
{
	// Inches - Feet - Meters - Centimeters - Millimeters
	private static double[] scaleValues = new double[5]
	{
		1,
		12,
		39.37,
		0.3937,
		0.03937
	};

	// Keeping the whole string for future reference
	// "\n\t\t\t{\n\t\t\t\t_class = \"ModelModifierList\"\n\t\t\t\tchildren = \n\t\t\t\t[\n\t\t\t\t\t{\n\t\t\t\t\t\t_class = \"ModelModifier_ScaleAndMirror\"\n\t\t\t\t\t\tscale = 0.3937\n\t\t\t\t\t\tmirror_x = false\n\t\t\t\t\t\tmirror_y = false\n\t\t\t\t\t\tmirror_z = false\n\t\t\t\t\t\tflip_bone_forward = false\n\t\t\t\t\t\tswap_left_and_right_bones = false\n\t\t\t\t\t},\n\t\t\t\t]\n\t\t\t},",

	private static string ScaleAndModifierStart =
		"\n\t\t\t{\n\t\t\t\t_class = \"ModelModifierList\"\n\t\t\t\tchildren = \n\t\t\t\t[\n\t\t\t\t\t{\n\t\t\t\t\t\t_class = \"ModelModifier_ScaleAndMirror\"\n\t\t\t\t\t\tscale = ";

	private static string ScaleAndModifierEnd =
		"\n\t\t\t\t\t\tmirror_x = false\n\t\t\t\t\t\tmirror_y = false\n\t\t\t\t\t\tmirror_z = false\n\t\t\t\t\t\tflip_bone_forward = false\n\t\t\t\t\t\tswap_left_and_right_bones = false\n\t\t\t\t\t},\n\t\t\t\t]\n\t\t\t},";
	
	private static readonly HashSet<string> MeshExtensions = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
	{
		"fbx",
		"obj",
		"dmx"
	};

	private static void AppendScaleAndMirror( Asset meshFile, int scaleUnitIndex )
	{
		string filename = meshFile.GetSourceFile(true);
		string txt = System.IO.File.ReadAllText( filename );
		if ( txt.First() == '<' )
		{
			// As we don't have access to EngineGlue.LoadKeyValues3( txt )
			// And EditorUtility.KeyValues3ToJson( txt ); returns null
			// we must manually insert the ScaleAndMirror modifier
			var lastChildIndex = txt.LastIndexOf( ',' );
			txt = txt.Insert( lastChildIndex + 1, ScaleAndModifierStart + scaleValues[scaleUnitIndex] + ScaleAndModifierEnd );
						
			System.IO.File.WriteAllText( filename, txt );
		}
	}
	
	static void RebuildCreateModelMenu( Menu model_menu, List<Asset> entries )
	{
		model_menu.AddOption( "ScaleAndMirror(Inches)", "open_in_new", () => entries.ForEach( asset => AppendScaleAndMirror( EditorUtility.CreateModelFromMeshFile( asset ), 0 ) ) );
		model_menu.AddOption( "ScaleAndMirror(Feet)", "open_in_new", () => entries.ForEach( asset => AppendScaleAndMirror( EditorUtility.CreateModelFromMeshFile( asset ), 1 ) ) );
		model_menu.AddOption( "ScaleAndMirror(Meters)", "open_in_new", () => entries.ForEach( asset => AppendScaleAndMirror( EditorUtility.CreateModelFromMeshFile( asset ), 2 ) ) );
		model_menu.AddOption( "ScaleAndMirror(Centimeters)", "open_in_new", () => entries.ForEach( asset => AppendScaleAndMirror( EditorUtility.CreateModelFromMeshFile( asset ), 3 ) ) );
		model_menu.AddOption( "ScaleAndMirror(Millimeters)", "open_in_new", () => entries.ForEach( asset => AppendScaleAndMirror( EditorUtility.CreateModelFromMeshFile( asset ), 4 ) ) );
	}

	[Event( "asset.contextmenu", Priority = 51 )]
	private static void OnMeshFileAssetContextExtension( AssetContextMenu e )
	{
		var meshes = e.SelectedList
			.Where( x => x.Asset is not null && MeshExtensions.Contains( x.AssetType.FileExtension ) )
			.Select( x => x.Asset )
			.ToList();

		if ( meshes.Count > 0 )
		{
			var model_menu = e.Menu.AddMenu( "Create model with..", "open_in_new" );
			RebuildCreateModelMenu( model_menu, meshes );
		}
	}
}
