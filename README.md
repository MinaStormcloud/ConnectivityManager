# ConnectivityManager
Please note that image files representing different levels of mobile network strength are not included. 
Add them to the Drawable folder in Visual Studio and update these tags in signal.xml accordingly:

<item android:drawable="@drawable/(filename)" android:minLevel="-1" android:maxLevel="0" />
<item android:drawable="@drawable/(filename)" android:minLevel="1" android:maxLevel="9" />
<item android:drawable="@drawable/(filename)" android:minLevel="10" android:maxLevel="19" />
<item android:drawable="@drawable/(filename)" android:minLevel="20" android:maxLevel="100" />

The WiFiActivity code displays a list of available networks, but it needs to be updated in order to connect the device to any of them.
