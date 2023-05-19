using System;
using Addressable;
using Il2CppSystem.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Utils;

namespace AssetRenderer
{
    public static class ReimplementedCg
    {
        public static GameObject GetCgSpinePrefab(int personalityId, bool isGacksung, Transform parent)
        {
            if (personalityId % 100 == 1)
                return null;
            var res = AddressableManager.Instance.LoadAssetSync<GameObject>("SpineIllust", $"{personalityId}{(isGacksung ? "_gacksung" : "_normal")}")
                .Item1;
            if (!(res != null))
                return null;
            res.transform.parent = parent;
            res.transform.localPosition = Vector3.zero;
            res.transform.localEulerAngles = Vector3.zero;
            res.transform.localScale = Vector3.one;
            return res;
        }

        public static GameObject SetCgData(
            int personalityId,
            bool isGacksung,
            Image img_illust,
            SPINE_LOCATION uiLocation,
            int orderInLayer = 100,
            Transform optionFxMask = null)
        {
            GameObject gameObject;
            gameObject = GetCgSpinePrefab(personalityId, isGacksung, img_illust.transform);
            if (gameObject != null)
            {
                var spineCgMask =
                    SingletonBehavior<UISpineCGMaskManager>.Instance.GetSpineCGMask(uiLocation);
                gameObject.transform.localScale = Vector3.one * spineCgMask.scaleFacter;
                gameObject.GetComponentInChildren<SortingGroup>().sortingOrder = orderInLayer;
                var materialList = new List<Material>();
                SkeletonGraphicCustomMaterials[] componentsInChildren1 =
                    gameObject.GetComponentsInChildren<SkeletonGraphicCustomMaterials>();
                var index1 = 0;
                for (var length = componentsInChildren1.Length; index1 < length; ++index1)
                {
                    var index2 = 0;
                    for (var count = componentsInChildren1[index1].CustomMaterialOverrides.Count;
                         index2 < count;
                         ++index2)
                        materialList.Add(componentsInChildren1[index1].CustomMaterialOverrides[(Index)index2].Cast<SkeletonGraphicCustomMaterials.AtlasMaterialOverride>()
                            .replacementMaterial);
                }

                Image[] componentsInChildren2 = gameObject.GetComponentsInChildren<Image>();
                var index3 = 0;
                for (var length = componentsInChildren2.Length; index3 < length; ++index3)
                    materialList.Add(componentsInChildren2[index3].material);
                var index4 = 0;
                for (var count = materialList.Count; index4 < count; ++index4)
                {
                    var material = materialList[(Index)index4].Cast<Material>();
                    material.SetFloat("_MaskRotate", spineCgMask.maskRotate);
                    material.SetFloat("_Mask_X1_EndLine", spineCgMask.mask_X1_EndLine);
                    material.SetFloat("_Mask_X2_EndLine", spineCgMask.mask_X2_EndLine);
                    material.SetFloat("_Mask_Y1_EndLine", spineCgMask.mask_Y1_EndLine);
                    material.SetFloat("_Mask_Y2_EndLine", spineCgMask.mask_Y2_EndLine);
                }

                if (optionFxMask != null)
                {
                    var transform = gameObject.transform.Find("FXMASK");
                    transform.parent = optionFxMask;
                    transform.position = gameObject.transform.position;
                    transform.GetComponent<SpriteMask>().enabled = false;
                    var allChildren = transform.GetChild(0).GetChild(0).GetAllChildren();
                    var index5 = 0;
                    for (var count = allChildren.Count; index5 < count; ++index5)
                        allChildren[(Index)index5].Cast<Transform>().localScale =
                            img_illust.rectTransform.localScale.x *
                            gameObject.transform.localScale.x * Vector3.one;
                }
            }

            //img_illust.sprite = this.GetCGData(personalityId, isGacksung);
            return gameObject;
        }
    }
}