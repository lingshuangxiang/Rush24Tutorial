using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TwentyFour.Scripts.Utilities
{
    public class GetItemPanel : MonoBehaviour
    {
        public RectTransform Light;
        public ParticleSystem StarEffect;
        public Button ExitButton;

        public GameObject GetItemItemPrefab;
        public RectTransform GetItemContainer;
        public List<GetItemData> GetItemDataList;

        public Text Title;

        private void Awake()
        {
        }

        public void PlayEffect(GetItemParams data)
        {
            gameObject.SetActive(true);
            GetItemDataList = data.GetItemDataList;
            Title.text = data.Title;
            RefreshItemList();
            StopAllCoroutines();
            StartCoroutine(Play());
        }

        public void OnClose()
        {
            gameObject.SetActive(false);
            GetItemContainer.gameObject.SetActive(false);
        }

        IEnumerator Play()
        {
            ExitButton.gameObject.SetActive(false);
            GetItemContainer.gameObject.SetActive(true);
            // GetItemContainer.transform.DOScale(1, 0.5f).From(0);

            Light.gameObject.SetActive(true);
            StarEffect.Play();
            Light.DOScale(1, 0.5f).From(0);
            Light.DOLocalRotate(new Vector3(0, 0, 360), 10, RotateMode.FastBeyond360).From(Vector3.zero)
                .SetEase(Ease.Linear).SetLoops(-1);
            yield return new WaitForSeconds(1f);
            ExitButton.gameObject.SetActive(true);
            ExitButton.GetComponentInChildren<Text>().DOFade(1, 1f).From(0);


        }

        void RefreshItemList()
        {
            foreach (var child in GetItemContainer.transform.GetComponentsInChildren<GetItemItem>())
            {
                Destroy(child.gameObject);
            }

            foreach (var product in GetItemDataList)
            {
                var item = Instantiate(GetItemItemPrefab, GetItemContainer).GetComponent<GetItemItem>();
                item.Init(product);
                // // item.ProductName.text = $"{product.DisplayName}:{product.ConsumeCount}/{product.LimitCount}"; 
                // // item.ProductSDK = product;
                // item.GetComponent<Button>().onClick.AddListener(() =>
                // {
                //     currentExpandedProduct = product;
                //     RefreshInfo(currentExpandedProduct);
                // });
            }
        }

    }
}