using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : SingletonMonoBehaviour<WeatherManager>
{
    public float temperature;
    public Seaon seaon;


}
public enum Seaon
{
    Spring,
    Summer,
    Autumn,
    Winter
}