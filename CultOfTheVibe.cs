using BepInEx;
//using HarmonyLib;
using UnityEngine;
using UnityEditor;
using Buttplug;
using System;
//using System.Threading.Tasks;

namespace CultOfTheVibe
{
    [BepInPlugin("Lilly.CultOfTheVibe", "Cult Of The Vibe", "1.0.0")]
    [BepInProcess("Cult Of The Lamb.exe")]

    public class CultOfTheVibe : BaseUnityPlugin
    {
        async void Awake()
        {
            await Run();
        }


        private async System.Threading.Tasks.Task Run()
        {
            var connector = new ButtplugEmbeddedConnectorOptions();
            var client = new ButtplugClient("Cult Of The Vibe Client");

            try
            {
                await client.ConnectAsync(connector);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Can't connect, exiting!");
                Debug.LogWarning($"Message: {ex.InnerException.Message}");
                return;
            }
            Debug.LogWarning("Client Connected");

        devicelost:
            await client.StartScanningAsync();
            while (client.Devices.Length == 0)
                await System.Threading.Tasks.Task.Delay(5000);
            await client.StopScanningAsync();
            Debug.LogWarning("Client currently knows about these devices:");
            foreach (var device in client.Devices)
            {
                Debug.LogWarning($"- {device.Name}");
            }

            foreach (var device in client.Devices)
            {
                Debug.LogWarning($"{device.Name} supports these messages:");
                foreach (var msgInfo in device.AllowedMessages)
                {
                    Debug.LogWarning($"- {msgInfo.Key.ToString()}");
                    if (msgInfo.Value.FeatureCount != 0)
                    {
                        Debug.LogWarning($" - Features: {msgInfo.Value.FeatureCount}");
                    }
                }
            }
            var testClientDevice = client.Devices;
            Debug.LogWarning("Sending commands");

            GameObject Player = null;
            while (true)
            {
                if (Player != null)
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(500);
                        for (int i = 0; i < testClientDevice.Length; i++)
                        {
                            await testClientDevice[i].SendVibrateCmd(1 - (Player.GetComponent<HealthPlayer>().HP / Player.GetComponent<HealthPlayer>().totalHP));
                        }
                        //Debug.LogWarning("power = " + (1 - (Player.GetComponent<HealthPlayer>().HP / Player.GetComponent<HealthPlayer>().totalHP)));
                    }
                    catch (ButtplugDeviceException)
                    {
                        Debug.LogWarning("device lost");
                        goto devicelost;
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("player lost");
                        Player = null;
                    }
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    Player = GameObject.Find("PlayerPrefab(Clone)");
                    if (Player != null && Player.activeSelf)
                        Debug.LogWarning("player found");
                }
            }
        }
    }
}
