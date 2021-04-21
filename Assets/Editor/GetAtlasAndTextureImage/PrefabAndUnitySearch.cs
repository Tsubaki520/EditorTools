using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Collections.Generic;
namespace GetAtlasImageAndTexture.Tool{
	public class PrefabAndUnitySearch{
		public PrefabAndUnitySearch(DReportAsset _dReportAsset,DIsPrefab _dIsPrefab){
			m_dReportAsset = _dReportAsset;
			m_dIsPrefab = _dIsPrefab;
		}
		public delegate void DReportAsset(string _sPath);
		public delegate bool DIsPrefab (string _sFilePath);
		private DReportAsset m_dReportAsset=null;
		private DIsPrefab m_dIsPrefab = null;

		private const string Replace_FilePath = "Assets";
		private List<System.Type> Image_Type=new List<System.Type>(){typeof(Texture),typeof(Texture2D),typeof(Sprite),typeof(UIAtlas)}; 
		private List<System.Type> Image_Array_Type=new List<System.Type>(){typeof(Texture[]),typeof(Texture2D[]),typeof(Sprite[]),typeof(UIAtlas[])}; 
		private const BindingFlags FindFieldFlag= BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

		public void PrefabAndSceneSearch(string[] _sarrPrefabPath){
			string PrefabPath = string.Empty;
			GameObject gobjPrefab = null;
            UIAtlas uIAtlas = null;
			for (int i = 0; i < _sarrPrefabPath.Length; i++) {
				PrefabPath = _sarrPrefabPath [i];
				if (m_dIsPrefab(PrefabPath)) {
                    uIAtlas = ((UIAtlas)AssetDatabase.LoadAssetAtPath(PrefabPath,typeof(UIAtlas)));
                    if (uIAtlas != null)
                    {
                        SendReport(uIAtlas);
                    }
                    else
                    {
                        gobjPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(GameObject));
                        if (gobjPrefab != null)
                        {
                            ProcessGame(gobjPrefab);
                        }
                    }
                    gobjPrefab = null;

                } else {
					SetScene (PrefabPath);
				}
			}
		}
		private void SetScene(string _sScenePath){
			EditorSceneManager.OpenScene (_sScenePath);
			List<GameObject> gobjlistScene =GetSceneGameObject();
			string sObjPath=string.Empty;
			for (int j = 0; j < gobjlistScene.Count; j++) {
				sObjPath = AssetDatabase.GetAssetPath (gobjlistScene [j]);
				if (!m_dIsPrefab(sObjPath)) {
					ProcessGame (gobjlistScene [j]);
				}
			}
            Resources.UnloadUnusedAssets();
			System.GC.Collect ();
		}
		private List<GameObject> GetSceneGameObject(){
			List<GameObject> gobjlistValue = new List<GameObject> ();
			Transform transObj;
			foreach (Object obj in Resources.FindObjectsOfTypeAll(typeof(Transform))) {
				transObj = obj as Transform; 
				transObj = transObj.root;
				if (!gobjlistValue.Contains(transObj.gameObject)) {
					gobjlistValue.Add (transObj.gameObject);
				}
			}
			return gobjlistValue;
		}
		private void ProcessGame(GameObject _gobjTarget){
			FindFieldImage (_gobjTarget);
		}
		private void FindFieldImage(GameObject _gobjTarget){
			List<Component> listCommponent = new List<Component> ();
			Component[] arrMono = _gobjTarget.GetComponents <Component>();
			if (arrMono != null) {
				listCommponent.AddRange (arrMono);
			}
			arrMono = _gobjTarget.GetComponentsInChildren <Component>(true);
			if (arrMono != null) {
				listCommponent.AddRange (arrMono);
			}
			FieldInfo[] arFields;
			Object[] objarrFieldValue = null;
			Object objFieldValue = null;
			foreach (Component script in listCommponent) {
				if (script != null) {
					arFields = script.GetType ().GetFields (FindFieldFlag);
					foreach (FieldInfo kField in arFields) {
						if (kField != null) {
							if (Image_Type.Contains (kField.FieldType)) {
								objFieldValue = (UnityEngine.Object)kField.GetValue (script);
								SendReport (objFieldValue);
							}
							else if (Image_Array_Type.Contains(kField.FieldType)) {
								objarrFieldValue = (UnityEngine.Object[])kField.GetValue (script);
								if (objarrFieldValue != null) {
									for (int i = 0; i < objarrFieldValue.Length; ++i) {
										SendReport (objarrFieldValue [i]);
									}
								}
							}
							ProcessMaterial (kField,script);
							ProcessNGUI (kField,script);
						}
					}
					if(script.GetType().Equals(typeof(ParticleSystemRenderer))){
						ParticleSystemRenderer render = (ParticleSystemRenderer)script;
						if(render.sharedMaterial!=null){
							SendReport (render.sharedMaterial.mainTexture);
						}
					}
				}
			}	
		}
		private void ProcessMaterial(FieldInfo _field,Component _script){
           

            if (!_script.GetType ().Equals (typeof(UIAtlas))) {
                string[] arrTextureName;
                if (_field.FieldType.Equals (typeof(Material))) {
					Material material = _field.GetValue (_script) as Material;
					if (material != null) {
                        arrTextureName = material.GetTexturePropertyNames();
                        for (int i=0;i< arrTextureName.Length;++i) {
                            SendReport(material.GetTexture(arrTextureName[i]));
                        }
					}
				}
				if (_field.FieldType.Equals (typeof(Material[]))) {
					Material[] arrMaterial = _field.GetValue (_script) as Material[];
					if (arrMaterial != null) {
						for (int i = 0; i < arrMaterial.Length; ++i) {
                            if (arrMaterial [i]!=null){
                                arrTextureName = arrMaterial[i].GetTexturePropertyNames();
                                for (int x=0;x<arrTextureName.Length;++x) {
                                    SendReport(arrMaterial[i].GetTexture(arrTextureName[x]));
                                }
							}
						}
					}
				}
			}
		}
		private void ProcessNGUI(FieldInfo _field,Component _script){
			if(_field.FieldType.Equals(typeof(UISprite))){
				UISprite uspScript = _field.GetValue (_script) as UISprite;
				if (uspScript != null) {
					SendReport (uspScript.atlas);
				}
			}
			else if(_field.FieldType.Equals(typeof(UISprite[]))){
				UISprite[] usparrScript = _field.GetValue (_script) as UISprite[];
				if(usparrScript!=null){
					for(int i=0;i<usparrScript.Length;++i){
						if (usparrScript [i] != null) {
							SendReport (usparrScript [i].atlas);
						}
					}
				}
			}
			else if(_field.FieldType.Equals(typeof(UI2DSprite))||_field.FieldType.Equals(typeof(UITexture))){
				UIBasicSprite ubsScript = _field.GetValue (_script) as UIBasicSprite;
				if (ubsScript != null) {
					SendReport (ubsScript.mainTexture);
				}
			}
			else if(_field.FieldType.Equals(typeof(UI2DSprite[]))||_field.FieldType.Equals(typeof(UITexture[]))){
				UIBasicSprite[] ubsarrScript = _field.GetValue (_script) as UIBasicSprite[];
				if(ubsarrScript!=null){
					for(int i=0;i<ubsarrScript.Length;++i){
						if (ubsarrScript [i] != null) {
							SendReport (ubsarrScript [i].mainTexture);
						}
					}
				}
			}
		}
		private void SendReport(Object _objTarget){
            if (_objTarget!=null) {
                string sFilePath = AssetDatabase.GetAssetPath(_objTarget);
                sFilePath = sFilePath.Replace(Application.dataPath, Replace_FilePath).Replace("\\", "/");
                if (m_dReportAsset != null && !string.IsNullOrEmpty(sFilePath) && !sFilePath.Contains("unity_builtin_extra")) {
                    m_dReportAsset(sFilePath);
                }
            }
		}
	}
}
