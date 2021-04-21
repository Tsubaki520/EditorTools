using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
namespace GetAtlasImageAndTexture.Tool{
	public class ProcessImage {
		public delegate bool DIsPrefab(string _sFilePath);
		public delegate void DReport (string _sFilePath);
		private DIsPrefab m_dIsPrefab = null;
		private DReport m_dWriteFail=null;
		private DReport m_dNoAddPadding = null;
		private DReport m_dWriteSuccesss = null;
		private DReport m_dAddPadding = null;
        private DReport m_dReplaceNameImage = null;
        private ProcessAtlas m_processAtlas = new ProcessAtlas();
		private string m_sExePath = string.Empty;
		private string m_sSavePath = string.Empty;

		private string m_sRootPath = string.Empty;
		private string m_sFileSavePath = string.Empty;
        private string m_sNotSaveName = string.Empty;
		private string[] m_sarrCutAtlasName = null;
		private UIAtlas m_uiaNowSelect = null;
		
        private byte[] m_bytarrImage = null;
        private string[] m_sArrToFilePath;
        private string m_sTargetDirPath;

        private const int AutoUnLoadAssetTime = 10;
        private const string Delimiter = "__";
		private const string SaveImageExtension = ".png";
        private List<string> NotSaveChar = new List<string>() { @"\", "/", "*", "?", ":", @"""", "<", ">", "|" };

		public ProcessImage(DIsPrefab _dIsPrefab,DReport _dWriteFail,DReport _dNoAddPadding,DReport _dWriteSuccesss,DReport _dAddPadding,DReport _dRepalceNameImage){
			m_dIsPrefab = _dIsPrefab;
			m_dWriteFail = _dWriteFail;
			m_dNoAddPadding = _dNoAddPadding;
			m_dWriteSuccesss = _dWriteSuccesss;
			m_dAddPadding = _dAddPadding;
            m_dReplaceNameImage = _dRepalceNameImage;

        }
		public void SetExePath(string _sExePath){
			m_sExePath = _sExePath;
		}
		public void SetSavePath(string _sSavePath){
			m_sSavePath = _sSavePath+"\\";
			m_sSavePath = m_sSavePath.Replace ("\\","/");
            
		}

		public void ProcessAndCopyImage(string[] _sarrFilePath){
            int iLogDoTime = 0;
			for (int i = 0; i < _sarrFilePath.Length; ++i) {
                if (iLogDoTime >= AutoUnLoadAssetTime)
                {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    iLogDoTime = 0;
                }
                if (!string.IsNullOrEmpty (_sarrFilePath [i])) {
                    if (m_dIsPrefab (_sarrFilePath [i])) {
                        ProcessAtlas(_sarrFilePath[i]);
					}
					else {
						CopyMove (_sarrFilePath[i],m_sSavePath+_sarrFilePath[i]);
					}
				}
                ++iLogDoTime;
            }
            m_sarrCutAtlasName = null;
            m_uiaNowSelect = null;
            m_sArrToFilePath = null;
            m_sTargetDirPath = string.Empty;
            m_sNotSaveName = string.Empty;
            m_sFileSavePath = string.Empty;
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
		private void ProcessAtlas(string _sPath){
			m_uiaNowSelect = AssetDatabase.LoadAssetAtPath (_sPath,typeof(UIAtlas)) as UIAtlas;
			if(m_uiaNowSelect!=null&&m_uiaNowSelect.texture!=null){
				SetAtlasSavePath (_sPath);
                m_processAtlas.SetAtlasImage(m_uiaNowSelect);
                for (int i=0;i<m_uiaNowSelect.spriteList.Count;++i){
                    m_sFileSavePath = GetSaveName(m_uiaNowSelect.spriteList[i].name);
                    m_bytarrImage = m_processAtlas.GetSpriteFromAtlas (m_uiaNowSelect.spriteList [i]);
                    if (m_bytarrImage != null)
                    {
                        File.WriteAllBytes(m_sFileSavePath, m_bytarrImage);
                        if (m_dWriteSuccesss != null)
                        {
                            m_dWriteSuccesss(m_sFileSavePath);
                        }
                        UseExeAddPadding(m_sFileSavePath, m_uiaNowSelect.spriteList[i]);
                        m_bytarrImage = null;
                    }
                    else
                    {
                        if (m_dWriteFail != null)
                        {
                            m_dWriteFail(m_sFileSavePath);
                        }
                    }
				}
                m_processAtlas.Relase();
            }
		}
        private string GetSaveName(string _sFileName) {
            m_sFileSavePath = _sFileName;
            if (IsCanntSave(m_sFileSavePath))
            {
                m_sNotSaveName = m_sFileSavePath;
                m_sFileSavePath = ReplaceCanntSaveChar(m_sFileSavePath);
                m_dReplaceNameImage(m_sRootPath + m_sNotSaveName + SaveImageExtension + " >>>>>> " + m_sRootPath + m_sFileSavePath + SaveImageExtension);
            }
            m_sFileSavePath = m_sRootPath + m_sFileSavePath + SaveImageExtension;
            return m_sFileSavePath;
        }
        private bool IsCanntSave(string _sCheckName) {
            bool bValue = false;
            for (int i=0;i<NotSaveChar.Count;++i) {
                if (_sCheckName.Contains(NotSaveChar[i])) {
                    bValue = true;
                    break;
                }
            }
            return bValue;
        }
        private string ReplaceCanntSaveChar(string _sReplaceName) {
            for (int i=0;i<NotSaveChar.Count;++i) {
                if (_sReplaceName.Contains(NotSaveChar[i])) {
                    _sReplaceName = _sReplaceName.Replace(NotSaveChar[i],i.ToString());
                }
            }
            return _sReplaceName;
        }
		private void UseExeAddPadding(string _sFileSavePath,UISpriteData _spriteData){
            if (_spriteData.paddingLeft != 0 && _spriteData.paddingTop != 0 && _spriteData.paddingRight != 0 && _spriteData.paddingBottom != 0)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = m_sExePath;
                processStartInfo.Arguments = "\"" + _sFileSavePath + "\" \"" + _sFileSavePath + "\" " + _spriteData.paddingLeft + " " + _spriteData.paddingTop + " " +
                _spriteData.paddingRight + " " + _spriteData.paddingBottom;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                process.StartInfo = processStartInfo;
                process.Start();
                if (m_dAddPadding != null)
                {
                    m_dAddPadding(_sFileSavePath);
                }
            }
            else
            {
                if (m_dNoAddPadding != null)
                {
                    m_dNoAddPadding(_sFileSavePath);
                }
            }
		}
        
		private void SetAtlasSavePath(string _sPath){
			m_sarrCutAtlasName = _sPath.Split ('/');
			m_sRootPath =_sPath.Replace (m_sarrCutAtlasName [m_sarrCutAtlasName.Length-1],"");
			if (m_uiaNowSelect.name.Contains (Delimiter)) {
				m_sarrCutAtlasName = m_uiaNowSelect.name.Split (new String[]{ Delimiter },StringSplitOptions.None);
				m_sRootPath = m_sSavePath+m_sRootPath + "TheOriginalImage/" + m_sarrCutAtlasName [0] + "/" + m_sarrCutAtlasName [1] + "/";
			} 
			else {
				m_sRootPath = m_sSavePath+m_sRootPath + "TheOriginalImage/" + m_uiaNowSelect.name + "/0/";
			}
			if(!Directory.Exists(m_sRootPath)){
				Directory.CreateDirectory (m_sRootPath);
			}
		}
		private void CopyMove (string FilePath, string ToFilePath)
		{
			if (FilePath != ToFilePath) {
				if(string.IsNullOrEmpty(FilePath)||string.IsNullOrEmpty(ToFilePath)){
					return;
				}
				try {
					FilePath = FilePath.Replace ("\\", "/");
					ToFilePath = ToFilePath.Replace ("\\", "/");
					m_sArrToFilePath = ToFilePath.Split ('/');
					m_sTargetDirPath= ToFilePath.Replace (m_sArrToFilePath [m_sArrToFilePath.Length - 1], "");
					if(!string.IsNullOrEmpty(m_sTargetDirPath)&&!Directory.Exists(m_sTargetDirPath)){
						Directory.CreateDirectory(m_sTargetDirPath);
					}
					File.Copy (FilePath, ToFilePath, true);
				}
				catch(System.Exception e) {
					Debug.Log (FilePath+"\n"+ToFilePath);
					Debug.LogError (e.ToString());
				}
			}
		}
	}
}