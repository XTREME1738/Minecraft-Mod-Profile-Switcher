   M S B A M L   `   `   `  ????  Minecraft_Profile_Switcher1    +Minecraft_Profile_Switcher.ModManagerWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          ? Title$  Mod Manager??5   	   $	??400??6   $	??900??6*   $	??700??66    ? Icon$ Resources/icon.ico??6E   .??5      ?  ? RowDefinitions 5	   
   ?? 5
      $??25 ?6   ?? 5      $??* ?6   ?? 5      $??25 ?6   .+?5   
   ??  ?? Orientation$ 
Horizontal=?6   .+?5      ?? -   5         ??Name$ ProfileGameVersionTextBox??5   c   $??
5, 0, 0, 0q?6   $??Right=?6,   $??Center=?6H   $??Profile Game Version:??6?   ?? 5      -   5        $ ProfileNameTextBox??5   c   $??20, 0, 0, 0q?6   $
??Left=?6-   $??Center=?6H   $??Profile Name:??6   5      ?? 5   
   -   5         ??Name$ ModListView??5      $??1??6X    ?? View 5      ?? 5      .P?5      ?? $??Mod Name??6%    ?? Width$	 290??6`    ?? DisplayMemberBinding 67   ?? *	ModName+6`   ?? 5      $??Mod Version??6%   $	 170??6f    6:   ?? *
ModVersion+6f   ?? 5      $??Mod Game Version??6%   $	 130??6o    6?   ?? *ModGameVersion+6o   ?? 5      $??Enabled??6%   $ 70??6_    66   ?? *	Enabled+6_   5      5      ?? 5   
   $??2??6   $ 
Horizontal=?6"   .+?5      ?? -   5        	 ? ToolTip$	 Add Mod JAR Or ZIP File??5   0   $??
5, 0, 0, 0q?6R   .??5      ?? $ 
Horizontal=?6   .+?5      ?? $??Resources/add.png??6   ?? 5       $??Add JAR/ZIP??6    $??5,0,0,0q?63   5!      5"      ?? 5#      -   5        $%	 Download Mod(s) From CurseForge??5#   3   $??
5, 0, 0, 0q?6]   .??5$      ?? $ 
Horizontal=?6   .+?5%      ?? $??Resources/curseforge.png??6   ?? 5&      $??Mod Downloader??6    $??5,0,0,0q?66   5'      5(      ?? 5)      -   5        
 ??Name$
 DeleteModButton??5)      $	 Delete Selected Mod??6L   $??
5, 0, 0, 0q?6j   {?.  .??5*      ?? $ 
Horizontal=?6   .+?5+      ?? $??Resources/delete.png??6   ?? 5,      $??Delete??6    $??5,0,0,0q?6.   5-      5.      ?? 5/      -   5        $
 DisableModButton??5/      $	 Disable Selected Mod??6N   $??
5, 0, 0, 0q?6m   {?.  .??50      ?? $ 
Horizontal=?6   .+?51      ?? $??Resources/disable.png??6   ?? 52      $??Disable??6    $??5,0,0,0q?6/   53      54      ?? 55      -	   5        $
 EnableModButton??55      $	 Enable Selected Mod??6L   $??
5, 0, 0, 0q?6j   {?.  .??56      ?? $ 
Horizontal=?6   .+?57      ?? $??Resources/use.png??6   ?? 58      $??Enable??6    $??5,0,0,0q?6.   59      5:      ?? 5;      -
   5        $
 ReloadModsButton??5;      $	 Reload Mods List??6N   $??
5, 0, 0, 0q?6i   {?.  .??5<      ?? $ 
Horizontal=?6   .+?5=      ?? $??Resources/reload.png??6   ?? 5>      $??Reload??6    $??5,0,0,0q?6.   5?      5@      ?? 5A      -   5        $	 Open Directory??5A   >   $??
5, 0, 0, 0q?6W   .??5B      ?? $ 
Horizontal=?6   .+?5C      ?? $??Resources/open.png??6   ?? 5D      $??Open Profile Directory??6    $??5,0,0,0q?6>   5E      5F      5G      ?? 5H   
   -   5        $??2??5H      $??25??68   $??Right=?6D   $??
0, 0, 5, 0q?6`   .??5I      ?? $ 
Horizontal=?6   .+?5J      ?? $??Resources/close.png??6   ?? 5K      $??Close??6   $??5,0,0,0q?6)   5L      5M      5N      5O      