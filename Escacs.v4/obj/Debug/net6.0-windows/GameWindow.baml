   M S B A M L   `   `   `  ����@  <Chess, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ChessChess  5   	       Chess.GameWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Chess     5   	   -   5          � WindowStartupLocation$  CenterScreen=�5	   	    � Title$ Chess��5
   	   $	��700��6   $	��700��6$   ���	#FF1C1C1C.��5      � -   5        $
��grid��5       � ShowGridLines$ False��6   $	��600��6-   $	��600��6:    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5       $��* �6   �� 5!      $��* �6   �� 5"      $��* �6   �� 5#      $��* �6   �� 5%      $��* �6   �� 5&      $��* �6   �� 5'      $��* �6   �� 5(      $��* �6   �� 5*      $��* �6   �� 5+      $��* �6   .+�5.   
   �� -   5        $��
timerBlack��5.      $
��0:00��6"   $��
ExtraBlack"�61   $��30(�6I   $��	Stormfaze)�6W   
�������$��8��6�   $��0��6�   $��2��6�   �� 5/   
   -   5        $��
timerWhite��5/      $
��0:00��6"   $��
ExtraBlack"�61   $��30(�6I   $��	Stormfaze)�6W   
�������$��8��6�   $��7��6�   $��2��6�   �� 52   
   -   5        $��txtBlock��52      $��9(�6$   $��Moves:��61   ${�False��6?   $��1��6Q   $��8��6_   $��2��6o   $��6��6�   $��
10,20,10,6q�6�   6�   �� 53   
   -   5        $��drawBtn��53      $��
Offer draw��6    $��9��68   $��2��6E   $��5��6Y   $��10,15,10,15q�6|   �� 54   
   -   5        $��	resignBtn��54      $��Resign��6"   $��9��66   $��2��6C   $��2��6W   $��10,15,10,15q�6v   55      56      