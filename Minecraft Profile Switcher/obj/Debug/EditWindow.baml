   M S B A M L   `   `   `  ????  Minecraft_Profile_Switcher+    %Minecraft_Profile_Switcher.EditWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     ? Title$  NameInputWindow??5   	   $	??150??6!   $	??300??6.    ? 
ResizeMode$ NoResize=?6:    ? WindowStartupLocation$ CenterScreen=?6P    ? Icon$ Resources/icon.ico??5   	   .??5	      ? $??10q?6    ? RowDefinitions 5
   
   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   .+?5   
   ?? -   5         ??Name$ QuestionLabel??5      $?? ??6'   $??Center=?62   ?? 5   
   -   5         ??Name$ ResponseTextBox??5      $??New Name??6+   $??1??6;   $??Stretch=?6H   $??25??6f   ?? 5   
   -   5        $ MinecraftVersionTextBox??5      $??2??63   $??Stretch=?6@   $??0,5,0,0q?6^   $??25??5      ?? 5   
    ?? Orientation$ 
Horizontal=?6   $??2??6.   $??Right=?6;   .+?5      ?? -   5         ??Name$ OkButton??5      $??OK??6'   $??25??6K   ?? 5      -   5        $ CancelButton??5      $??Cancel??6+   $??25??6W   5      5      5      