using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUIでRenderer,ParticleSystemを扱うコンポーネント
/// </summary>
public class UIUnityRenderer : UIWidget
{
    // 指定されているMaterialの値を直接変更します（NGUIのレイアウトにより変更点が都度発生するので注意）
    public bool allowSharedMaterial = false;

    private Material[] mMats;
    private Renderer mRenderer;
    private int mRenderQueue = -1;

#if UNITY_EDITOR
    public int renderQueue
    {
        get { return mRenderQueue; }
    }

    public int materialsCount
    {
        get
        {
            if (allowSharedMaterial)
                return 1;
            else
                return mMats == null ? 0 : mMats.Length;
        }
    }

    public string GetMaterialName(int index)
    {
        if (allowSharedMaterial)
        {
            if (index != 0) return "";

            Material sharedMat = cachedRenderer.sharedMaterial;
            return sharedMat == null ? "" : sharedMat.name;
        }
        else
        {
            if (mMats == null) return "";
            if (index < 0 || index > mMats.Length) return "";

            Material mat = mMats[index];
            return mat == null ? "" : mat.name;
        }
    }

    public bool ResetMaterial()
    {
        mMats = null;
        return material != null;
    }
#endif

    public Renderer cachedRenderer
    {
        get
        {
            if (mRenderer == null)
                mRenderer = GetComponent<Renderer>();
            return mRenderer;
        }
    }

    /// <summary>
    /// Material used by Renderer.
    /// </summary>
    public override Material material
    {
        get
        {
            if (ExistSharedMaterial0() == false)
            {
                Debug.LogError("Renderer or Material is not found.");
                return null;
            }

            if (allowSharedMaterial == false)
            {
                if (CheckMaterial(mMats) == false)
                {
                    int validCount = 0;
                    for (int i = 0; i < cachedRenderer.sharedMaterials.Length; i++)
                    {
                        if (cachedRenderer.sharedMaterials[i] != null)
                            validCount++;
                    }
                    mMats = new Material[validCount];
                    for (int i = 0; i < cachedRenderer.sharedMaterials.Length; i++)
                    {
                        if (cachedRenderer.sharedMaterials[i] != null)
                        {
                            mMats[i] = new Material(cachedRenderer.sharedMaterials[i]);
                            mMats[i].name = mMats[i].name + "(Copy)";
                        }
                    }
                }

                if (CheckMaterial(mMats))
                {
                    if (Application.isPlaying)
                    {
                        //if ( cachedRenderer.materials != mMats ) Modify By Terry 2015/05/12 avoid memory leak
                        cachedRenderer.materials = mMats;
                    }
                }
                return mMats[0]; //NGUIには0番目で登録する
            }
            else
                return cachedRenderer.sharedMaterials[0]; //SharedMaterial index0をNGUIに登録する
        }

        set
        {
            throw new System.NotImplementedException(GetType() + " has no material setter");
        }
    }

    /// <summary>
    /// Shader used by Renderer material.
    /// </summary>
    public override Shader shader
    {
        get
        {
            if (allowSharedMaterial == false)
            {
                if (CheckMaterial(mMats))
                    return mMats[0].shader;
            }
            else
            {
                if (ExistSharedMaterial0())
                    return cachedRenderer.sharedMaterials[0].shader;
            }
            return null;
        }
        set { throw new System.NotImplementedException(GetType() + " has no shader setter"); }
    }

    /// <summary>
    /// SharedMaterialに一つでもマテリアルが存在するかチェック
    /// </summary>
    private bool ExistSharedMaterial0()
    {
        if (cachedRenderer != null && CheckMaterial(cachedRenderer.sharedMaterials))
            return true;
        return false;
    }

    /// <summary>
    /// マテリアルが存在するかチェック
    /// </summary>
    private bool CheckMaterial(Material[] mats)
    {
        if (mats != null && mats.Length > 0)
        {
            if (mats[0] != null)
                return true;
            /*
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                    return false;
            }
            */
            return true;
        }
        return false;
    }

    void OnDestroy()
    {
        if (mMats != null)
        {
            for (int i = 0; i < mMats.Length; i++)
            {
                DestroyImmediate(mMats[i]);
                mMats[i] = null;
            }
            mMats = null;
        }
    }

    private void OnWillRenderObject()
    {
        if (allowSharedMaterial == false)
        {
            if (CheckMaterial(mMats) && this.drawCall != null)
            {
                mRenderQueue = drawCall.finalRenderQueue;

                for (int i = 0; i < mMats.Length; i++)
                {
                    if (mMats[i].renderQueue != mRenderQueue)
                        mMats[i].renderQueue = mRenderQueue;
                }
            }
        }
        else
        {
            if (ExistSharedMaterial0() && drawCall != null)
            {
                mRenderQueue = drawCall.finalRenderQueue;

                for (int i = 0; i < cachedRenderer.sharedMaterials.Length; i++)
                {
                    if (cachedRenderer.sharedMaterials[i] != null)
                    {
                        cachedRenderer.sharedMaterials[i].renderQueue = mRenderQueue;
                    }
                }
            }
        }

        if (drawCall != null)
            cachedRenderer.sortingOrder = drawCall.sortingOrder;
    }

    /// <summary> Dammy Mesh </summary>
    public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        verts.Add(new Vector3(10000f, 10000f));
        verts.Add(new Vector3(10000f, 10000f));
        verts.Add(new Vector3(10000f, 10000f));
        verts.Add(new Vector3(10000f, 10000f));

        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(1f, 0f));

        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
    }
}