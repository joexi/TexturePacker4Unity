using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class TexturePackerEditor
{

	[MenuItem("Tools/OpenTexturePacker")]
	static void OpenTexturePacker ()
	{
		TexturePackerWindow.Show ();
	}

	[MenuItem("Tools/PackSelected")]
	static void PackSelected ()
	{

	}
}

public class TexturePackerWindow : EditorWindow
{

	public static void Show ()
	{
		TexturePackerWindow window = EditorWindow.GetWindowWithRect<TexturePackerWindow>(new Rect(0, 0, 600, 400));
        window.Init();
		window.Show (true);
	}

	public void Init() 
	{

	}

	private string	 	m_atlas_name;
	private string	 	m_atlas_path;
	private Vector2 	m_scroll_position;
	private bool 		m_rotate = false;

	public void OnGUI() 
	{
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("AtlasName:", GUILayout.Width (100));
		m_atlas_name = EditorGUILayout.TextField (m_atlas_name, GUILayout.MinWidth (200));
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Rotate:", GUILayout.Width (100));
		m_rotate = EditorGUILayout.Toggle (m_rotate, GUILayout.MinWidth (200));
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Output:", GUILayout.Width (100));
		EditorGUILayout.TextField (m_atlas_path, GUILayout.MinWidth (200));

		if (GUILayout.Button ("...", GUILayout.Width (100))) {
			m_atlas_path = EditorUtility.SaveFolderPanel (m_atlas_name, "", m_atlas_name + ".prefab");
		}
		EditorGUILayout.EndHorizontal ();

		m_scroll_position = EditorGUILayout.BeginScrollView (m_scroll_position);
		UnityEngine.Object[] SelectedAsset = Selection.GetFiltered (typeof(UnityEngine.Texture2D), SelectionMode.DeepAssets);
		for (int i = 0; i < SelectedAsset.Length; i++) {
			Object obj = SelectedAsset[i];
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (obj.name);
			EditorGUILayout.EndHorizontal ();
		}
		EditorGUILayout.EndScrollView ();


		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Pack")) {
			Pack ();
		}
		EditorGUILayout.EndHorizontal ();
		Repaint ();
	}

	private const string CMD = "{4} --sheet {0}  --texture-format png --data {1} {3} --format unity {2}";
	public void Pack() 
	{
		UnityEngine.Object[] SelectedAsset = Selection.GetFiltered (typeof(UnityEngine.Texture2D), SelectionMode.DeepAssets);

		string file_list = "";
		for (int i = 0; i< SelectedAsset.Length; i++) {
			Object obj = SelectedAsset[i];
			string path = Application.dataPath + AssetDatabase.GetAssetPath (obj).Replace("Assets", "");
			if (file_list.Length > 0) {
				file_list += " ";
			}
			file_list += path;	
		}
		if (SelectedAsset.Length == 0) {
			EditorUtility.DisplayDialog ("Error", "Please chose your textures!", "ok");
			return;
		}

		if (string.IsNullOrEmpty (m_atlas_path) ||
		    string.IsNullOrEmpty (m_atlas_name)) {
			EditorUtility.DisplayDialog ("Error", "Please set the atlas name and output path!", "ok");
			return;
		}
		string shell_path = Application.dataPath + "/Editor/tp.sh";

		string disable_rotation = "--disable-rotation";
		string output_path_png = m_atlas_path + "/" + m_atlas_name + ".png";
		string output_path_txt = m_atlas_path + "/" + m_atlas_name + ".txt";

		string tp_path = Application.dataPath.Replace("Assets", "") + "TexturePacker.app/Contents/MacOS/TexturePacker";
		Debug.Log (tp_path);
		string cmd = string.Format (CMD, output_path_png, output_path_txt, file_list, disable_rotation, tp_path);

		File.WriteAllText (shell_path, cmd);
		System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo("sh", shell_path);
		proc.UseShellExecute = false;
		System.Diagnostics.Process.Start(proc);
	}
}
