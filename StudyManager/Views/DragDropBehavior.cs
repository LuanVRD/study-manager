using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StudyManager.Views
{
    public static class DragDropBehavior
    {
        private class DragState
        {
            public Point StartPoint { get; set; }
            public bool IsDragging { get; set; }
            public bool DraggedDistanceExceeded { get; set; }
        }

        private static readonly DependencyProperty DragStateProperty =
            DependencyProperty.RegisterAttached(
                "DragState",
                typeof(DragState),
                typeof(DragDropBehavior),
                new PropertyMetadata(null));

        private static DragState GetDragState(DependencyObject obj)
        {
            var state = (DragState)obj.GetValue(DragStateProperty);
            if (state == null)
            {
                state = new DragState();
                obj.SetValue(DragStateProperty, state);
            }
            return state;
        }

        #region IsDragSource Property
        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.RegisterAttached(
                "IsDragSource",
                typeof(bool),
                typeof(DragDropBehavior),
                new PropertyMetadata(false, OnIsDragSourceChanged));

        public static bool GetIsDragSource(DependencyObject obj) => (bool)obj.GetValue(IsDragSourceProperty);
        public static void SetIsDragSource(DependencyObject obj, bool value) => obj.SetValue(IsDragSourceProperty, value);

        private static void OnIsDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseMove += Element_PreviewMouseMove;
                    element.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
                    element.LostMouseCapture += Element_LostMouseCapture;
                }
                else
                {
                    element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseMove -= Element_PreviewMouseMove;
                    element.PreviewMouseLeftButtonUp -= Element_PreviewMouseLeftButtonUp;
                    element.LostMouseCapture -= Element_LostMouseCapture;
                }
            }
        }
        #endregion

        #region IsDropTarget Property
        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.RegisterAttached(
                "IsDropTarget",
                typeof(bool),
                typeof(DragDropBehavior),
                new PropertyMetadata(false, OnIsDropTargetChanged));

        public static bool GetIsDropTarget(DependencyObject obj) => (bool)obj.GetValue(IsDropTargetProperty);
        public static void SetIsDropTarget(DependencyObject obj, bool value) => obj.SetValue(IsDropTargetProperty, value);

        private static void OnIsDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.AllowDrop = true;
                    element.DragEnter += Element_DragEnter;
                    element.DragLeave += Element_DragLeave;
                    element.DragOver += Element_DragOver;
                    element.Drop += Element_Drop;
                }
                else
                {
                    element.AllowDrop = false;
                    element.DragEnter -= Element_DragEnter;
                    element.DragLeave -= Element_DragLeave;
                    element.DragOver -= Element_DragOver;
                    element.Drop -= Element_Drop;
                }
            }
        }
        #endregion

        #region DragGroup Property
        public static readonly DependencyProperty DragGroupProperty =
            DependencyProperty.RegisterAttached(
                "DragGroup",
                typeof(string),
                typeof(DragDropBehavior),
                new PropertyMetadata(string.Empty));

        public static string GetDragGroup(DependencyObject obj) => (string)obj.GetValue(DragGroupProperty);
        public static void SetDragGroup(DependencyObject obj, string value) => obj.SetValue(DragGroupProperty, value);
        #endregion

        #region DropCommand Property
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached(
                "DropCommand",
                typeof(ICommand),
                typeof(DragDropBehavior),
                new PropertyMetadata(null));

        public static ICommand GetDropCommand(DependencyObject obj) => (ICommand)obj.GetValue(DropCommandProperty);
        public static void SetDropCommand(DependencyObject obj, ICommand value) => obj.SetValue(DropCommandProperty, value);
        #endregion

        #region IsDragOverItem Property
        public static readonly DependencyProperty IsDragOverItemProperty =
            DependencyProperty.RegisterAttached(
                "IsDragOverItem",
                typeof(bool),
                typeof(DragDropBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetIsDragOverItem(DependencyObject obj) => (bool)obj.GetValue(IsDragOverItemProperty);
        public static void SetIsDragOverItem(DependencyObject obj, bool value) => obj.SetValue(IsDragOverItemProperty, value);
        #endregion

        #region ClickCommand Property
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.RegisterAttached(
                "ClickCommand",
                typeof(ICommand),
                typeof(DragDropBehavior),
                new PropertyMetadata(null));

        public static ICommand GetClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(ClickCommandProperty);
        public static void SetClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(ClickCommandProperty, value);
        #endregion

        #region ClickCommandParameter Property
        public static readonly DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "ClickCommandParameter",
                typeof(object),
                typeof(DragDropBehavior),
                new PropertyMetadata(null));

        public static object GetClickCommandParameter(DependencyObject obj) => obj.GetValue(ClickCommandParameterProperty);
        public static void SetClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(ClickCommandParameterProperty, value);
        #endregion

        // Drag Source Events
        private static bool IsInsideNestedDragSource(DependencyObject child, DependencyObject? ancestor)
        {
            if (child == null || ancestor == null) return false;
            DependencyObject current = child;
            while (current != null && current != ancestor)
            {
                if (current is UIElement element && GetIsDragSource(element))
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private static void Element_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                var state = GetDragState(obj);
                state.IsDragging = false;
            }
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject depObj)
            {
                if (IsInsideNestedDragSource(depObj, sender as DependencyObject))
                {
                    return; // Yield to the child drag source
                }

                // Do not initiate drag if user clicked interactive controls
                if (FindVisualParent<Button>(depObj) != null ||
                    FindVisualParent<TextBox>(depObj) != null ||
                    FindVisualParent<CheckBox>(depObj) != null ||
                    FindVisualParent<RadioButton>(depObj) != null)
                {
                    if (sender is DependencyObject obj)
                    {
                        var state = GetDragState(obj);
                        state.IsDragging = false;
                    }
                    return;
                }
            }

            if (sender is UIElement element)
            {
                var state = GetDragState(element);
                state.StartPoint = e.GetPosition(null);
                state.IsDragging = true;
                state.DraggedDistanceExceeded = false;
                element.CaptureMouse();
            }
        }

        private static void Element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var state = GetDragState(element);
                if (!state.IsDragging || e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }

                Point currentPoint = e.GetPosition(null);
                Vector diff = state.StartPoint - currentPoint;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    state.DraggedDistanceExceeded = true;
                    element.ReleaseMouseCapture();
                    state.IsDragging = false;

                    string dragGroup = GetDragGroup(element);
                    object itemData = element.DataContext;

                    if (!string.IsNullOrEmpty(dragGroup) && itemData != null)
                    {
                        var dragData = new DataObject(dragGroup, itemData);
                        DragDrop.DoDragDrop(element, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private static void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                var state = GetDragState(element);
                bool wasDragging = state.IsDragging;
                bool wasDistanceExceeded = state.DraggedDistanceExceeded;

                element.ReleaseMouseCapture();
                state.IsDragging = false;

                if (wasDragging && !wasDistanceExceeded)
                {
                    ICommand clickCommand = GetClickCommand(element);
                    if (clickCommand != null)
                    {
                        object parameter = GetClickCommandParameter(element);
                        if (clickCommand.CanExecute(parameter))
                        {
                            clickCommand.Execute(parameter);
                        }
                    }
                    else if (element is Expander expander)
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                        e.Handled = true;
                    }
                }
            }
        }

        // Drop Target Events
        private static void Element_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                string dragGroup = GetDragGroup(obj);
                if (!string.IsNullOrEmpty(dragGroup) && e.Data.GetDataPresent(dragGroup))
                {
                    SetIsDragOverItem(obj, true);
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                }
            }
        }

        private static void Element_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                SetIsDragOverItem(obj, false);
            }
        }

        private static void Element_DragOver(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                string dragGroup = GetDragGroup(obj);
                if (!string.IsNullOrEmpty(dragGroup) && e.Data.GetDataPresent(dragGroup))
                {
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private static void Element_Drop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                SetIsDragOverItem(element, false);
                string dragGroup = GetDragGroup(element);

                if (!string.IsNullOrEmpty(dragGroup) && e.Data.GetDataPresent(dragGroup))
                {
                    object sourceItem = e.Data.GetData(dragGroup);
                    object targetItem = element.DataContext;

                    if (sourceItem != null && targetItem != null && sourceItem != targetItem)
                    {
                        var itemsControl = FindParentItemsControl(element);
                        if (itemsControl != null && itemsControl.ItemsSource is IList list)
                        {
                            int sourceIndex = list.IndexOf(sourceItem);
                            int targetIndex = list.IndexOf(targetItem);

                            if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
                            {
                                // Attempt to invoke 'Move' method if supported (like ObservableCollection)
                                var moveMethod = list.GetType().GetMethod("Move", new Type[] { typeof(int), typeof(int) });
                                if (moveMethod != null)
                                {
                                    moveMethod.Invoke(list, new object[] { sourceIndex, targetIndex });
                                }
                                else
                                {
                                    // Fallback for regular lists
                                    list.RemoveAt(sourceIndex);
                                    list.Insert(targetIndex, sourceItem);
                                }

                                // Execute DropCommand if set
                                ICommand dropCommand = GetDropCommand(element);
                                if (dropCommand != null && dropCommand.CanExecute(null))
                                {
                                    dropCommand.Execute(null);
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
            }
        }

        // Helpers
        private static ItemsControl? FindParentItemsControl(DependencyObject element)
        {
            var parent = VisualTreeHelper.GetParent(element);
            while (parent != null)
            {
                if (parent is ItemsControl itemsControl)
                {
                    return itemsControl;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindVisualParent<T>(parentObject);
        }
    }
}
