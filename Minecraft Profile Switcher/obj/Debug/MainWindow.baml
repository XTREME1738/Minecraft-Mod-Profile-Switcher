   M S B A M L   `   `   `  ????  Minecraft_Profile_Switcher+    %Minecraft_Profile_Switcher.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     ? Title$$  Minecraft Mod Profile Switcher??5   	   $	??170??60   $	??400??6=    ? 
ResizeMode$ NoResize=?6I    ? WindowStartupLocation$ CenterScreen=?6_    ? Icon$ Resources/icon.ico??6?   .??5      ? $??10q?6    ? RowDefinitions 5	   
   ?? 5
      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   .+?5   
   ??  ?? Orientation$ 
Horizontal=?6   $??1??6.   $??Center=?6;   $??Center=?6X   .+?5      ?  ? ColumnDefinitions 5      ?? 5      $	??275 ?6'   ?? 5      $??30 ?6'   ?? 5      $??30 ?6'   ?? 5      $??30 ?6'   .+?5      ?? -   5         ??Name$ ProfileComboBox??5      $??0??6h   $??0,2,0,0q?6x   ?? 5      -   5         ??Name$ 	AddButton??5      $??1??6D   $	??2,0q?6T   .??5      ?? $??Resources/add.png??6   69   5      ?? 5      -   5        $ 
EditButton??5      $??2??6F   $	??2,0q?6V   .??5      ?? $ ??Resources/edit_profile.png??6   6B   5      ?? 5      -   5        $ DeleteButton??5      $??3??6J   $	??2,0q?6Z   .??5       ?? $??Resources/delete.png??6   6<   5!      5"      5#      ?? 5$   
   $??2??6   $ 
Horizontal=?6"   $??Center=?6;   $??
0, 5, 0, 0q?6X   .+?5%      ?? -   5        $ ActivateButton??5%      $	??5,0q?6-   $??25??6W   .??5&      ?? $ 
Horizontal=?6   .+?5'      ?? $??Resources/use.png??6   69   ?? 5(      -   5        	 ??Name$	 ActivateButtonLabel??5(       $??5,0,0,0q?6=   
Activate6X   5)      5*      ?? 5+      -   5        $ ManageModsButton??5+      $	??5,0q?6/   $??25??6[   .??5,      ?? $ 
Horizontal=?6   .+?5-      ?? $??Resources/edit_mods.png??6   6?   ?? 5.      $??5,0,0,0q?6    Manage Mods6>   5/      50      ?? 51      -   5        $  OpenProfileDirectoryButton??51      $	??5,0q?69   $??25??6i   .??52      ?? $ 
Horizontal=?6   .+?53      ?? $??Resources/open.png??6   6:   ?? 54      $??5,0,0,0q?6    Open Profile Directory6I   55      56      57      ?? 58   
   -	   5        $??Center=?58      $??
0, 5, 0, 0q?6.   $??25??6e   $??3??6q   .??59      ?? $ 
Horizontal=?6   .+?5:      ?? $??Resources/use.png??6   65   ?? 5;      $??5,0,0,0q?6   Check For Updates6@   5<      5=      ?? 5>   
   -
   5        
 ??Name$
 ActiveProfileLabel??5>      $??Active Profile: None??6,   $??4??6K   $??Center=?6X   5?      5@      