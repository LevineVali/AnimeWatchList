   M S B A M L   `   `   `  ����I  EAnimeWatchList, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:AnimeWatchListAnimeWatchList  5   	   ,    &AnimeWatchList.SaveLoadAnimeListWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:AnimeWatchList     5   	   -   5          �Name$  SaveLoadWindow��5   a    � Title$  ��6	   $	��250��6   $	��250��6"   $	��400��6/   $	��400��6>   ���	#FF1D1D1D � WindowStyle$
 None=�5	   	    � AllowsTransparency$
 True��6   ���	#FF3D3D3D$��2q�5
   !    � WindowStartupLocation$ CenterScreen=�5   	    � 	Resources 5         FocusVisual �� Property �� Value �� StrokeDashArray	 J� SnapsToDevicePixels  Button.Static.Background
 �� Color  Button.Static.Border  Button.MouseOver.Background  Button.MouseOver.Border  Button.Pressed.Background  Button.Pressed.Border  Button.Disabled.Background  Button.Disabled.Border 	 Button.Disabled.Foreground 
 ButtonStyleOwn �� 
TargetType �� 
TargetType ��Name ��Name �� Triggers P� Property P� Value �� 
TargetName��%�  &	        &	 �     &	 �     &	 �     &	     &	 ?    &	 b    &	 �    &	 �    &		 �    &	
 �    7   7  7  7  7  7  7  7  7  7 	 �� 5   
   .�5      ��  � �� 5      �� 5      .R�5      �� $��2q�6$   $	 1 2R�6/   $
	 true��6E   $��1��6`   #��� ��6t   5      5      5      �� 5   
   $
 	#FFDDDDDD��6;   �� 5   
   $
 	#FF707070��67   �� 5   
   $
 	#FF5D5D5D��6>   �� 5   
   $
 	#FF5D5D5D��6:   �� 5   
   $
 	#FF5E5E5E��6<   �� 5   
   $
 	#FF5E5E5E��68   �� 5   
   $
 	#FF2D2D2D��6=   �� 5   
   $
 	#FFADB2B5��69   �� 5   
   $
 	#FFAAAAAA��6=   �� 5    
   " ��6'   .�5!      ��  �@�FocusVisualStyle8   61   �� 5"       � ��8  6+   �� 5#       � ��8  6,   �� 5$       � ��# � ��6+   �� 5%       � ��$ 1q�60   �� 5&       � ��$ Center=�6;   �� 5'       � ��$ Center=�69   �� 5(       � ��$ 1q�6(   �� 5)       � �� 5*      �� 5+      " ��6&   .R�5,      �� $ border��6!   #��z��61   #��z��6[   #��z��6�   $
	 true��6�   ._�5-      �� $ contentPresenter��6/   $|�False��6I   #��z��6[   #��z��6�   $
��True��6�   #	 z	 6�   #��z��6  5.       5/      P� 50       �@��IsDefaulted$
 true��6=   . �51   "   �� $ border��6@   # � ��6T    � ��52      P� 53       �@J�IsMouseOver$
 true��6=   . �54   "   �� $ border��6?   8   � ��6S   �� 55   "   $ border��6@   8   � ��6T   56      P� 57       � ��$
 true��6;   . �58   "   �� $ border��6?   8   � ��6S   �� 59   "   $ border��6@   8   � ��6T   5:      P� 5;       � {�$ false��6;   . �5<   "   �� $ border��6?   8   � ��6S   �� 5=   "   $ border��6@   8   � ��6T   �� 5>   "   $ contentPresenter��6K   8 	  � ��6i   5?      5A      5C      5D      5E      '  !System.Windows.Shell.WindowChrome   WindowChrome 5G        5H   
      CaptionHeight$ 25Q�6   .��5K      �  � RowDefinitions 5L   
   �� 5M      $��25 �6   �� 5N      �� 5O      $��22 �6   .+�5S   
   ]� $��25��6   .+�5T      �� -   5         ��Name$ button_Close��5T      $��Right=�6+   $��25��6B   ���	#FF1D1D1D   IsHitTestVisibleInChrome$
 True��6d   $��0q�6�   .��5U      �� -   5         ��Name$ image_Close��5U      $��15��6-   $��15��69   5V      �� 5W      -   5        $ button_MinMax��5W      $��Right=�6,   $��25��6C   ���	#FF1D1D1D$
 True��6e   $��0q�6�   .��5X      �� -   5        $ image_MinMax��5X      $��15��6.   $��15��6:   5Y      �� 5Z      -   5        $ button_Minimized��5Z      $��Right=�6/   $��25��6F   ���	#FF1D1D1D$
 True��6h   $��0q�6�   .��5[      �� -   5        $ image_Minimized��5[      $��15��61   $��15��6=   5\      �� 5]      $
��Left=�6   $��MediumWatchList��6*   
�������6D   5^      �� 5`   
   $��1��6   $
��Auto=�6$   $
��Auto=�6I   .��5a      ]� -   5         ]�Name$ dockpanel_FileList��5a      $��False��64   $��10q�6J   5b      �� 5d   
   -	   5        #��� 
 5d   )   $��2��6R   $��20��6_   $��20��6j   $��0,0,2,2q�6v   ���	#FF1D1D1D$��0q�6�   $��Right=�6�   .��5e      �� -
   5        $ image_Directory��5e      $��19��6-   $��19��68   5f      5g      5h      