   M S B A M L   `   `   `  ����@  <Chess, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ChessChess  5   	       Chess.WaitingRoom   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Chess     5   	   -   5          � Title$  WaitingRoom��5   	   $	��450��6   $	��800��6*   ���#212121.��5	      � -   5         � RowDefinitions 5
   
   �� 5      $��20 �6   �� 5      $��30 �6   �� 5      -   5         �Name$ midrow��5      $��* �6*   �� 5      $��20 �6   �� 5      $��20 �6   �� 5      $��20 �6    � ColumnDefinitions 5   
   �� 5      $��20 �6   �� 5      $	��150 �6   �� 5      -   5        $ midcol��5      $��* �6-   �� 5      $	��150 �6   �� 5      $��20 �6   .+�5   
   �� -   5        $��btnFindMatch��5      $��Find match...��6%   $��3��5   ,   $��2��69   $	��150��5      ���#212121���#EDEDED���#4A4A4A6G   �� 5   
   -   5        $��btnStopFindMatch��5      $��Stop Finding��6)   $��4��5   0   $��2��6=   $	��150��5      ���#212121���#EDEDED���#4A4A4A$y�	Collapsed=�6]   �� 5    
   -   5        $��labelLoadingGame��5       $��Finding Match ��6(   $��16(�5!      $
��Bold"�6   ���#EDEDED$��2��6E   $��2��5"       K� ZIndex$ 2��6    $	��200��61   $��50��5#      $y�Hidden=�6   �� 5$   
   -   5        $��gamesListTitle��5$      $	��150��6&   $��12(�62   $
��Bold"�6@   $��
GAMES LIST��6R   ���#EDEDED$��1��6|   $��2��6�   $ 1��6�   �� 5%   
   -	   5        $��lblPlayersListTitle��5%      $	��150��6+   $��12(�67   $
��Bold"�6E   $��Players Online��6W   ���#EDEDED$��1��6�   $��3��6�   $ 1��6�   �� 5&   
   -
   5        $��	gamesList��5&      $��2��6#   $��2��60   $ 1��6@   ���#2D2D2D���#EDEDED���#4A4A4A6{   �� 5'   
   -   5        $��playersList��5'      $��2��6%   $��3��62   ���#2D2D2D���#EDEDED���#4A4A4A6l   �� 5(   
   -   5        $��lstRankList��5(      $��2��6%   $��1��62   $	��Top=�5)      $��5,5,5,0q�6+   $	��180��6<   ���#2D2D2D���#EDEDED���#4A4A4A6s   �� 5*   
   -   5        $��lstPlayerInfo��5*      $��2��6'   $��1��64   $��Bottom=�5+      $��10q�6/   $��70��6;   ���#2D2D2D���#EDEDED���#4A4A4A6q   �� 5,   
   $	��100��6   $��12(�6   $
��Bold"�6*   $
��User��5-      ���#EDEDED$��2��64   $��1��6A   $ 1��6Q   $��25,190,25,85q�6b   �� 5.   
   $	��100��6   $��12(�6   $
��Bold"�6*   $��Ranking��5/      ���#EDEDED$��1��67   $��1��6D   $ 1��6T   50      51      