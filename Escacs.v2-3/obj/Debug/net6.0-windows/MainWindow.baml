   M S B A M L   `   `   `  ����@  <Chess, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ChessChess  5   	       Chess.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Chess     5   	     � WindowStartupLocation$  CenterScreen=�5   	   ���#202020 � Title$$ Welcome to Chess by Roger Roca��5
   	   $	��650��60   $	��700��6=   .��5      � -   5        $
��grid��5      $	��600��6   $	��600��6$   $��Center=�60   $��Center=�6M    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��
timerBlack��5      $��Chess��6"   $��
ExtraBlack"�62   $��36(�6J   
�������$��	Stormfaze)�6k   $��0��6�   $��Center=�6�   $
��0,20q�6�   �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   $
��0,10q�6X   .+�5      �� -   5        $��lblLogin��5      $��Login:��6$   $��16(�65   
�������$��0,0,10,0q�6V   �� 5      -   5        $��txtLogin��5   3   $	��150��6   $��	RogerRoca��6"   $
��10,0q�6C   6Q   5      �� 5    
   $��3��6   $ 
Horizontal=�6"   $��Center=�6;   $
��0,10q�6X   .+�5!      �� -   5        $��lblPass��5!      $��	Password:��6#   $��16(�67   
�������$��0,0,10,0q�6X   �� 5"      -   5        $��txtPass��5"   H   $	��150��6   $��WebDings)�6"   $��20031994��68   $
��10,0q�6W   6e   5#      �� 5&   
   $��4��6   $ 
Horizontal=�6"   $��Center=�6;   $y�Hidden=�6X   $
��0,10q�6l   .+�5'      �� -   5        $��hostIP��5'      $��host IP:��6"   
�������$��0,0,10,0q�6H   �� 5(      -   5        $��txtHost��5(   *   $��192.168.0.11��6   6*   5)      �� 5,   
   $��5��6   $ 
Horizontal=�6"   $��Center=�6;   $
��0,20q�6X   .+�5-      �� -	   5        $��loginBtn��5-      $��Login��6%   $	��120��65   $��40��6A   $��0,0,10,0q�6a   �� 5.      -
   5        $��playBtn��5.      $
��Play��6$   $	��120��63   $��40��6?   $y�Hidden=�6K   $��0,0,10,0q�6r   �� 5/      -   5        $��createAccBtn��5/      $��Create Account��6)   $	��150��6B   $��40��6N   6Z   50      52      53      