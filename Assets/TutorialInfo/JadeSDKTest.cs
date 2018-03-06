
using UnityEngine;

public class JadeSDKTest : MonoBehaviour
{
    void Awake()
    {
#if UNITY_STANDALONE_WIN
        Screen.SetResolution(Screen.currentResolution.width,
                              Screen.currentResolution.height, true);
#endif
    }

    void Start()
	{
        /*RuyiSDK sdk = RuyiSDK.CreateInstance(new RuyiSDKContext(){endpoint=RuyiSDKContext.Endpoint.Console});

		var msg = new StorageLayerCustomerCollectionRequest();
		msg.Customer = new StorageLayerCustomerCollection()
		{
			Id = 1111,
			Name = "John Doe",
			PhoneNumber = "8000-0000",
			IsActive = true
		};

		var rep = sdk.Storage.CreateCustomerCollection(msg);
		Debug.Log(rep.ToString());

        var result = sdk.L10NService.SwitchLanguage("en-US", true, false);
        Debug.Log(result);*/
    }
}