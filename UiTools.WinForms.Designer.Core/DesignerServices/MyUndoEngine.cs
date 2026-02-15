using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace UiTools.WinForms.Designer.Core
{
    public class MyUndoEngine : UndoEngine
    {
        private readonly Stack<UndoUnit> undoStack = new Stack<UndoUnit>();
        private readonly Stack<UndoUnit> redoStack = new Stack<UndoUnit>();

        public event EventHandler<string> UndoStackChanged;
        public event EventHandler<string> RedoStackChanged;

        public MyUndoEngine(IServiceProvider provider) : base(provider)
        {
        }

        // Method to update the state of UI commands
        private void UpdateCommandsState()
        {
            var ims = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (ims != null)
            {
                var undoCmd = ims.FindCommand(StandardCommands.Undo);
                if (undoCmd != null)
                    undoCmd.Enabled = undoStack.Count > 0;

                var redoCmd = ims.FindCommand(StandardCommands.Redo);
                if (redoCmd != null)
                    redoCmd.Enabled = redoStack.Count > 0;
            }
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                try
                {
                    UndoUnit unit = undoStack.Pop();
                    unit.Undo();
                    redoStack.Push(unit);
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, ex.Message, ex);
                }
                finally
                {
                    OnUndoStackChanged();
                    OnRedoStackChanged();
                    UpdateCommandsState();
                }
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                try
                {
                    UndoUnit unit = redoStack.Pop();
                    unit.Undo(); // UndoUnit.Undo() implements *toggle* logic
                    undoStack.Push(unit);
                }
                catch (Exception ex)
                {
                    MessageLogger.LogError(this, ex.Message, ex);
                }
                finally
                {
                    OnUndoStackChanged();
                    OnRedoStackChanged();
                    UpdateCommandsState();
                }
            }
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnUndoStackChanged();
            OnRedoStackChanged();
            UpdateCommandsState();
        }

        protected override void AddUndoUnit(UndoUnit unit)
        {
            undoStack.Push(unit);

            // IMPORTANT: If the user performs a new action, the Redo stack must be cleared (standard Windows behavior):
            redoStack.Clear();

            OnUndoStackChanged();
            OnRedoStackChanged();
            UpdateCommandsState();
        }

        private string GetLastUndoName() => undoStack.Count > 0 ? undoStack.Peek()?.Name : null;
        private string GetLastRedoName() => redoStack.Count > 0 ? redoStack.Peek()?.Name : null;

        private void OnUndoStackChanged()
        {
            UndoStackChanged?.Invoke(this, GetLastUndoName());
        }

        private void OnRedoStackChanged()
        {
            RedoStackChanged?.Invoke(this, GetLastRedoName());
        }
    }
}
