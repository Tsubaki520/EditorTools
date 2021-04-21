using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GetAtlasImageAndTexture.Tool;
using System.IO;
namespace GetAtlasImageAndTexture{
	public class GetAtlasAndTextureImageWindows : EditorWindow {
		private string m_sSavePath = "D:\\MLG_LOG";
		private string m_sExePath=string.Empty;
		private bool m_bHasExePath = false;
		private string m_sSearchPath=string.Empty;
		private bool m_bNeedRelease = false;
		private string[] m_sarrExeCutPath = null;
		private const string DefaultSavePath = "D:\\MLG_LOG";
		private const string PaddingEXEName = "PNGPadding2.exe";
		private const string PlayerPrefabExeSavePathKey = "PngPaddingEXE";

		private WriteLogTool m_writeLog=new WriteLogTool();
		private FileMgr m_fileMgr = null;
        private GUIStyle m_uisBox = null;

		[MenuItem ("Tools/Get Atlas And Texture Image")]
		private static void OpenUITranslator ()
		{
            GetWindow(typeof(GetAtlasAndTextureImageWindows));
            
		}
        private void OnEnable()
        {
            m_uisBox = new GUIStyle("box");
            m_uisBox.normal.textColor = Color.white;
            Repaint();
        }
        private void OnGUI(){
			EditorGUILayout.BeginVertical ();
			if (GUILayout.Button ("圖片存檔位置:" + m_sSavePath, m_uisBox)) {
				m_sSavePath = EditorUtility.OpenFolderPanel ("選擇要存檔位置", "D:\\", "");
				if (string.IsNullOrEmpty (m_sSavePath)) {
					m_sSavePath = DefaultSavePath;
				}
			}
			m_bNeedRelease = EditorGUILayout.Toggle ("是否需要Release", m_bNeedRelease);
			if(!m_bHasExePath) {
				SetExePath ();
			}
			if(m_bHasExePath) {
				if(m_fileMgr==null){
					m_fileMgr = new FileMgr (m_writeLog.Report);
					m_fileMgr.SetExePath (m_sExePath);
				}
				if (GUILayout.Button ("複製所有檔案的圖片")) {
                    if (EditorUtility.DisplayDialog("複製所有其他語系檔案", "確定要複製所有檔案的圖片?\n會很久喔", "開始","取消")) {
                        m_sSearchPath = Application.dataPath;
                        m_fileMgr.FindAndCopyNeedFiel(m_sSearchPath, m_bNeedRelease, m_sSavePath);
                        m_writeLog.WriteLog(m_sSavePath);
                    }
                }
				if (GUILayout.Button ("複製選擇的資料夾裡的圖片")) {
					m_sSearchPath = EditorUtility.OpenFolderPanel ("選擇要複製的資料夾", Application.dataPath, "");
					if (!string.IsNullOrEmpty (m_sSearchPath)) {
						m_fileMgr.FindAndCopyNeedFiel (m_sSearchPath, m_bNeedRelease,m_sSavePath);
						m_writeLog.WriteLog (m_sSavePath);
					}
				}
			} 
			EditorGUILayout.EndVertical ();
		}

		private void SetExePath(){
			if (PlayerPrefs.HasKey (PlayerPrefabExeSavePathKey)) {
				m_sExePath = PlayerPrefs.GetString (PlayerPrefabExeSavePathKey);
			}
			if (IsExePath ()) {
				PlayerPrefs.SetString (PlayerPrefabExeSavePathKey, m_sExePath);
				PlayerPrefs.Save ();
				if(m_fileMgr==null){
					m_fileMgr = new FileMgr (m_writeLog.Report);
				}
				m_fileMgr.SetExePath (m_sExePath);
				Repaint ();
				m_bHasExePath = true;
			} else {
				if (GUILayout.Button ("尋找 " + PaddingEXEName)) {
					m_sExePath = EditorUtility.OpenFilePanel ("需要" + PaddingEXEName, Application.dataPath, "exe");
				}
			}
		}
		private bool IsExePath(){
			bool bValue = false;
			if (!string.IsNullOrEmpty (m_sExePath)) {
				if (File.Exists (m_sExePath)) {
					m_sExePath = m_sExePath.Replace ("\\", "/");
					m_sarrExeCutPath = m_sExePath.Split ('/');
					bValue = m_sarrExeCutPath [m_sarrExeCutPath.Length - 1].Equals (PaddingEXEName);
				}
			}
			return bValue;
		}
	}
}
