# AutoHook - Final Fantasy XIV Fishing Plugin

AutoHook is a FFXIV plugin that assists you while fishing

## Help us with localization
https://crowdin.com/project/autohook-plugin-localization

## Installation

```
https://raw.githubusercontent.com/InitialDet/MyDalamudPlugins/main/pluginmaster.json
```
* Copy the link above
* Open your Plugin Installer Window and click Settings
* Go to the Experimental Tab
* Paste the link into the Custom Plugin Repositories, and click the + on the right to add it.
* Save and close.
* Search and Install AutoHook from the Plugin Installer.
* Enjoy

![image](https://github.com/InitialDet/AutoHook/assets/13919114/3811f164-eb56-4e8e-b9d2-7604518393e4)


# Features


- **Conditional Hooking:** Customize hooking behavior based on factors like the current bait/mooch, player effects such as Patience I/II, Fisher's Intuition, and more.

- **Auto Casting Actions and Items:** Automate the casting of actions and use of items, including Auto Cast Line, Auto Mooch, Cordials, Thaliak's Favor, and more.

- **Auto Spear Fishing (Experimental Feature):** Select the size and speed of the fish you're aiming to catch, and let the plugin handle the rest. Please note that this feature is experimental and may be subject to updates and improvements.

# Important Tabs


### Bait Tab

In the "Bait" sub-tab, you can set conditions for normal hooking. For example, you can specify that the plugin should only hook Weak fish bites (displayed as a single exclamation mark "!"), Strong Bites ("!!"), or Legendary Bites ("!!!"). You can also set Min. and Max. time limite, so it'll never hook before the Min. Time, and stop the attempt if the Max. Time is reached.

### Mooch Tab

The "Mooch" sub-tab functions similarly to the "Bait" tab but focuses on customizing conditions for mooching actions. In FFXIV, a fish being mooched is considered a type of bait. In this tab, you can set specific conditions for mooching actions.

Both the "Bait" and "Mooch" tabs serve the same purpose but are separated for better organization.

### Auto Cast Tab
A simple tab: select one or more actions from the list, and the plugin will cast it for you.

### Fish Tab

In the "Fish" sub-tab, you can setup different behaviors when catching a specific fish. For example, when catching a "Merlthor Goby", you can set the plugin to cast either Surface Slap or Identical cast, mooch it (if available), or swap to another custom preset after it being caught X amount of times, for a more complex configuration.

# Custom Preset Tab
The "Custom Preset" tab allows you to customize how the plugin behaves when fishing with baits and mooches you manually added. Some random examples:

- **Bait: Gold Salmon Roe**
  - You can specify that the plugin should only hook Legendary Bites (!!!) when using the "Gold Salmon Roe" bait.

- **Bait: Pill Bug**
  - For the "Pill Bug", you might configure the plugin to only hook Weak Bites (!) that happens **before** 20 seconds.

- **Mooch: Merlthor Goby**
  - When mooching "Merlthor Goby", you can configure to hook Strong Bites (!!) only **after** 15 seconds have passed.

# Default Preset Tab

In the "Default" tab of AutoHook, you can configure the hooking and mooching behavior for **ALL** baits and mooches. This means that the settings you define here will apply universally to every bait and mooch, regardless of their specific names.
