   M S B A M L   `   `   `  ����H  DPDF-MultiTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:PDF_MultiToolPDF_MultiTool  5   	       PDF_MultiTool.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:PDF_MultiTool     5   	     � Title$  PDF MultiTool��5   	   $	��450��6   $	��600��6,   .��5	      �  � RowDefinitions 5
   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ OperationComboBox��5      $	��200��5      $��10q�5      $
��Left=�5      .@�5      �� .��6   	Clear PDF6&   �� 5      .��6   Convert PDF Details60   5      �� 5   
   -   5         ��Name$ ClearPdfPanel��5      $��1��5      $��10q�5      $y�	Collapsed=�5      .+�5      �� -   5        $��
Select PDF��5      $
��10,5q�5      $
��Left=�5       $��0,0,0,10q�5!      �� 5"      -   5         ��Name$ SelectedPdfPath��5"      $��No file selected��5#      $��0,0,0,10q�5$      �� 5%      -   5        $��	Clear PDF��5%      $
��10,5q�5'      $
��Left=�5(      5)      �� 5,   
   -   5        $ ConvertPdfPanel��5,      $��1��5-      $��10q�5.      $y�	Collapsed=�5/      .+�50      �� -   5        $��Select Old PDF��50      $
��10,5q�52      $
��Left=�53      $��0,0,0,10q�54      �� 55      -   5        $ 
OldPdfPath��55      $��No old file selected��56      $��0,0,0,10q�57      �� 58      -	   5        $��Select New PDF��58      $
��10,5q�5:      $
��Left=�5;      $��0,0,0,10q�5<      �� 5=      -
   5        $ 
NewPdfPath��5=      $��No new file selected��5>      $��0,0,0,10q�5?      �� 5@      -   5        $��Convert��5@      $
��10,5q�5B      $
��Left=�5C      5D      5E      5F      