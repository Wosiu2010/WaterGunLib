using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterGunLib
{
    public class GrassShaderRestorer : MonoBehaviour
    {
        public List<Material> materialsToApplyShader = new List<Material>();
        public bool ApplyShaderOnAwake = true;
        public void Awake()
        {
            if (ApplyShaderOnAwake)
            {
                Shader grassWaving = GetGrassShader();


                if (materialsToApplyShader.Count == 0) return;
                foreach (var mat in materialsToApplyShader)
                {

                    if (grassWaving == null)
                        Debug.LogError("Shader missing");
                    else
                        mat.shader = grassWaving;
                }
            }
        }

        public Shader GetGrassShader()
        {
            return Shader.Find("Shader Graphs/WavingGrass");
        }
    }
}
