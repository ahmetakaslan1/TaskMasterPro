import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import type { BoardDetailDto, TaskDto } from "../services/boardService";
import { boardService } from "../services/boardService";
import { taskService } from "../services/taskService";
import { columnService } from "../services/columnService";
import type { CommentDto } from "../services/commentService";
import { commentService } from "../services/commentService";

import {
  DndContext,
  DragOverlay,
  closestCorners,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  useDroppable,
} from "@dnd-kit/core";
import type {
  DragStartEvent,
  DragOverEvent,
  DragEndEvent,
} from "@dnd-kit/core";
import {
  SortableContext,
  arrayMove,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";

// --- Sortable Task Item Component ---
interface SortableTaskProps {
  task: TaskDto;
  onClick: (task: TaskDto) => void;
}

const SortableTask = ({ task, onClick }: SortableTaskProps) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: task.id, data: { type: "Task", task } });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.3 : 1,
  };

  return (
    <div
      ref={setNodeRef}
      {...attributes}
      {...listeners}
      onClick={() => onClick(task)}
      className="p-4 bg-black/40 border border-white/5 hover:border-white/20 rounded-xl cursor-grab active:cursor-grabbing shadow-md group touch-none"
      style={{ ...style, borderTop: `3px solid ${task.color || "#3b82f6"}` }}
    >
      <h4 className="font-medium text-white mb-1 group-hover:text-blue-300 transition-colors">
        {task.title}
      </h4>
      {task.description && (
        <p className="text-sm text-gray-400 line-clamp-2">{task.description}</p>
      )}
    </div>
  );
};

// --- Main Board Detail Component ---
function DroppableColumn({ column, handleTaskClick, openTaskModal, onEditColumn, onDeleteColumn }: any) {
  const { setNodeRef } = useDroppable({
    id: column.id,
    data: {
      type: 'Column',
      column,
    },
  });

  return (
    <div 
      ref={setNodeRef}
      className="flex-shrink-0 w-80 max-h-full flex flex-col bg-white/5 border border-white/10 rounded-2xl backdrop-blur-xl"
    >
      <div className="p-4 border-b border-white/10 flex items-center justify-between group/col">
        <div className="flex items-center space-x-3">
          <h3 className="font-semibold text-lg">{column.title}</h3>
          <span className="text-xs bg-black/30 px-2 py-1 rounded-full text-gray-400">
            {column.tasks.length}
          </span>
        </div>
        <div className="opacity-0 group-hover/col:opacity-100 flex space-x-1 transition-opacity">
          <button onClick={() => onEditColumn(column)} className="p-1 hover:bg-white/10 rounded text-gray-400 hover:text-white" title="Düzenle">✏️</button>
          <button onClick={() => onDeleteColumn(column.id, column.title)} className="p-1 hover:bg-white/10 rounded text-gray-400 hover:text-red-400" title="Sil">🗑️</button>
        </div>
      </div>

      <div className="p-3 flex-1 overflow-y-auto space-y-3 custom-scrollbar min-h-[150px]">
        <SortableContext items={column.tasks.map((t: any) => t.id)} strategy={verticalListSortingStrategy}>
          {column.tasks.map((task: any) => (
            <SortableTask key={task.id} task={task} onClick={handleTaskClick} />
          ))}
        </SortableContext>

        <button 
          onClick={() => openTaskModal(column.id)}
          className="w-full py-3 mt-2 rounded-xl border border-dashed border-white/20 text-gray-400 hover:text-white hover:border-white/40 hover:bg-white/5 transition-colors flex items-center justify-center space-x-2"
        >
          <span>+ Görev Ekle</span>
        </button>
      </div>
    </div>
  );
}

const BoardDetail = () => {
  const { id } = useParams<{ id: string }>();
  const [board, setBoard] = useState<BoardDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  // Drag & Drop State
  const [activeTask, setActiveTask] = useState<TaskDto | null>(null);

  // Modals State
  const [isColumnModalOpen, setIsColumnModalOpen] = useState(false);
  const [newColumnTitle, setNewColumnTitle] = useState("");
  const [editingColumn, setEditingColumn] = useState<any>(null);
  const [editColumnTitle, setEditColumnTitle] = useState("");

  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [newTaskTitle, setNewTaskTitle] = useState("");
  const [newTaskDesc, setNewTaskDesc] = useState("");
  const [targetColumnId, setTargetColumnId] = useState("");

  const [selectedTask, setSelectedTask] = useState<TaskDto | null>(null);
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [newComment, setNewComment] = useState("");

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  );

  useEffect(() => {
    if (id) {
      fetchBoardDetails(id);
    }
  }, [id]);

  const fetchBoardDetails = async (boardId: string) => {
    try {
      const data = await boardService.getById(boardId);
      data.columns.sort((a, b) => a.order - b.order);
      data.columns.forEach((col) =>
        col.tasks.sort((a, b) => a.order - b.order),
      );
      setBoard(data);
    } catch (err: any) {
      setError("Pano detayları yüklenirken hata oluştu.");
    } finally {
      setIsLoading(false);
    }
  };

  // --- Column Modal Handlers ---
  const handleCreateColumn = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newColumnTitle.trim() || !id) return;
    try {
      await columnService.create(id, newColumnTitle);
      setIsColumnModalOpen(false);
      setNewColumnTitle("");
      fetchBoardDetails(id);
    } catch (err) {
      alert("Sütun oluşturulamadı.");
    }
  };

  const handleUpdateColumn = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editColumnTitle.trim() || !id || !editingColumn) return;
    try {
      await columnService.update(editingColumn.id, editColumnTitle);
      setEditingColumn(null);
      fetchBoardDetails(id);
    } catch (err) {
      alert("Sütun güncellenirken hata oluştu.");
    }
  };

  const handleDeleteColumn = async (columnId: string, title: string) => {
    if (window.confirm(`"${title}" sütununu ve içindeki tüm görevleri silmek istediğinize emin misiniz?`)) {
      try {
        await columnService.delete(columnId);
        if (id) fetchBoardDetails(id);
      } catch (err) {
        alert("Sütun silinirken hata oluştu.");
      }
    }
  };

  const openEditColumnModal = (column: any) => {
    setEditingColumn(column);
    setEditColumnTitle(column.title);
  };

  // --- Task Modal Handlers ---
  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTaskTitle.trim() || !targetColumnId || !id) return;
    try {
      await taskService.create(targetColumnId, newTaskTitle, newTaskDesc);
      setIsTaskModalOpen(false);
      setNewTaskTitle("");
      setNewTaskDesc("");
      fetchBoardDetails(id);
    } catch (err) {
      alert("Görev oluşturulamadı.");
    }
  };

  const openTaskModal = (colId: string) => {
    setTargetColumnId(colId);
    setIsTaskModalOpen(true);
  };

  // --- Task Details & Comments Handlers ---
  const handleTaskClick = async (task: TaskDto) => {
    setSelectedTask(task);
    try {
      const data = await commentService.getByTaskId(task.id);
      setComments(data);
    } catch (err) {
      console.error("Yorumlar çekilemedi");
    }
  };

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !selectedTask) return;
    try {
      await commentService.create(selectedTask.id, newComment);
      setNewComment("");
      // refresh comments
      const data = await commentService.getByTaskId(selectedTask.id);
      setComments(data);
    } catch (err) {
      alert("Yorum eklenemedi.");
    }
  };

  const handleDeleteTask = async () => {
    if (!selectedTask || !id) return;
    if (window.confirm("Bu görevi silmek istediğinize emin misiniz?")) {
      try {
        await taskService.delete(selectedTask.id);
        setSelectedTask(null);
        fetchBoardDetails(id);
      } catch (err) {
        alert("Görev silinemedi.");
      }
    }
  };

  // --- Drag and Drop Handlers ---
  const onDragStart = (event: DragStartEvent) => {
    const { active } = event;
    const task = board?.columns
      .flatMap((c) => c.tasks)
      .find((t) => t.id === active.id);
    if (task) setActiveTask(task);
  };

  const onDragOver = (event: DragOverEvent) => {
    const { active, over } = event;
    if (!over || !board) return;

    const activeId = active.id;
    const overId = over.id;

    if (activeId === overId) return;

    const isActiveTask = active.data.current?.type === "Task";
    const isOverTask = over.data.current?.type === "Task";

    if (!isActiveTask) return;

    if (isActiveTask && (isOverTask || over.data.current?.type === "Column")) {
      setBoard((prev) => {
        if (!prev) return prev;
        const newBoard = { ...prev };

        let activeColIdx = newBoard.columns.findIndex((c) =>
          c.tasks.some((t) => t.id === activeId),
        );
        let overColIdx = isOverTask
          ? newBoard.columns.findIndex((c) =>
              c.tasks.some((t) => t.id === overId),
            )
          : newBoard.columns.findIndex((c) => c.id === overId);

        if (activeColIdx === -1 || overColIdx === -1) return prev;

        if (activeColIdx !== overColIdx) {
          const activeTasks = [...newBoard.columns[activeColIdx].tasks];
          const overTasks = [...newBoard.columns[overColIdx].tasks];

          const activeTaskIdx = activeTasks.findIndex((t) => t.id === activeId);
          let overTaskIdx = isOverTask
            ? overTasks.findIndex((t) => t.id === overId)
            : overTasks.length;

          const [movedTask] = activeTasks.splice(activeTaskIdx, 1);
          movedTask.columnId = newBoard.columns[overColIdx].id;

          if (overTaskIdx === -1) overTaskIdx = overTasks.length;
          overTasks.splice(overTaskIdx, 0, movedTask);

          newBoard.columns[activeColIdx] = {
            ...newBoard.columns[activeColIdx],
            tasks: activeTasks,
          };
          newBoard.columns[overColIdx] = {
            ...newBoard.columns[overColIdx],
            tasks: overTasks,
          };

          return newBoard;
        }
        return prev;
      });
    }
  };

  const onDragEnd = async (event: DragEndEvent) => {
    setActiveTask(null);
    const { active, over } = event;
    if (!over || !board) return;

    const activeId = active.id;
    const overId = over.id;
    if (activeId === overId) return;

    const previousBoard = JSON.parse(JSON.stringify(board));
    const isOverTask = over.data.current?.type === "Task";

    let finalColumnId = "";
    let finalOrder = 0;
    let newBoardState = { ...board };

    let activeColIdx = newBoardState.columns.findIndex((c) =>
      c.tasks.some((t) => t.id === activeId),
    );
    let overColIdx = newBoardState.columns.findIndex((c) => c.id === overId);

    if (isOverTask) {
      overColIdx = newBoardState.columns.findIndex((c) =>
        c.tasks.some((t) => t.id === overId),
      );
    }

    if (activeColIdx !== -1 && overColIdx !== -1) {
      const activeTasks = [...newBoardState.columns[activeColIdx].tasks];
      const overTasks =
        activeColIdx === overColIdx
          ? activeTasks
          : [...newBoardState.columns[overColIdx].tasks];

      const activeTaskIdx = activeTasks.findIndex((t) => t.id === activeId);
      let overTaskIdx = isOverTask
        ? overTasks.findIndex((t) => t.id === overId)
        : overTasks.length;

      if (activeColIdx === overColIdx) {
        const newTasks = arrayMove(activeTasks, activeTaskIdx, overTaskIdx);
        newTasks.forEach((t, idx) => (t.order = idx));
        finalColumnId = newBoardState.columns[activeColIdx].id;
        finalOrder = overTaskIdx;
        newBoardState.columns[activeColIdx].tasks = newTasks;
      } else {
        overTasks.forEach((t, idx) => (t.order = idx));
        finalColumnId = newBoardState.columns[overColIdx].id;
        const currentIdx = overTasks.findIndex((t) => t.id === activeId);
        finalOrder = currentIdx !== -1 ? currentIdx : 0;
        newBoardState.columns[overColIdx].tasks = overTasks;
      }

      setBoard(newBoardState);
    }

    try {
      if (finalColumnId) {
        await taskService.move(activeId as string, finalColumnId, finalOrder);
      }
    } catch (err) {
      setBoard(previousBoard);
      setError("Görev taşınırken bir hata oluştu, geri alındı.");
      setTimeout(() => setError(""), 3000);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-gray-900 bg-opacity-95 text-white overflow-hidden relative">
      {error && board && (
        <div className="absolute top-4 left-1/2 -translate-x-1/2 z-50 bg-red-500 text-white px-6 py-2 rounded-lg shadow-xl">
          {error}
        </div>
      )}

      <header className="flex-none flex items-center justify-between px-8 py-4 bg-white/5 border-b border-white/10 backdrop-blur-md">
        <div className="flex items-center space-x-4">
          <Link
            to="/boards"
            className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-white/10 transition-colors text-gray-400 hover:text-white"
          >
            ←
          </Link>
          <h1 className="text-2xl font-bold text-white">{board?.title}</h1>
        </div>
        <div className="flex space-x-3">
          <button
            onClick={() => setIsColumnModalOpen(true)}
            className="px-4 py-2 bg-white/5 hover:bg-white/10 border border-white/10 rounded-lg text-sm font-medium transition-colors"
          >
            + Sütun Ekle
          </button>
        </div>
      </header>

      <main className="flex-1 overflow-x-auto p-8">
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragStart={onDragStart}
          onDragOver={onDragOver}
          onDragEnd={onDragEnd}
        >
          <div className="flex items-start space-x-6 h-full pb-4">
            {board?.columns.map((column) => (
              <DroppableColumn 
                key={column.id} 
                column={column} 
                handleTaskClick={handleTaskClick} 
                openTaskModal={openTaskModal} 
                onEditColumn={openEditColumnModal}
                onDeleteColumn={handleDeleteColumn}
              />
            ))}
          </div>

          <DragOverlay>
            {activeTask ? (
              <div
                className="p-4 bg-black border border-blue-500/50 rounded-xl shadow-2xl opacity-80 rotate-2"
                style={{
                  borderTop: `3px solid ${activeTask.color || "#3b82f6"}`,
                }}
              >
                <h4 className="font-medium text-white mb-1">
                  {activeTask.title}
                </h4>
                {activeTask.description && (
                  <p className="text-sm text-gray-400 line-clamp-2">
                    {activeTask.description}
                  </p>
                )}
              </div>
            ) : null}
          </DragOverlay>
        </DndContext>
      </main>

      {/* Column Modal */}
      {isColumnModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Yeni Sütun Ekle</h3>
            <form onSubmit={handleCreateColumn}>
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-1">
                  Sütun Adı
                </label>
                <input
                  type="text"
                  required
                  value={newColumnTitle}
                  onChange={(e) => setNewColumnTitle(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
                  placeholder="Örn: Test Edilenler"
                />
              </div>
              <div className="flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setIsColumnModalOpen(false)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                >
                  Ekle
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Task Modal */}
      {isTaskModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Yeni Görev Ekle</h3>
            <form onSubmit={handleCreateTask}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-1">
                  Görev Adı
                </label>
                <input
                  type="text"
                  required
                  value={newTaskTitle}
                  onChange={(e) => setNewTaskTitle(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
                  placeholder="Görev başlığı"
                />
              </div>
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-1">
                  Açıklama
                </label>
                <textarea
                  value={newTaskDesc}
                  onChange={(e) => setNewTaskDesc(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500 min-h-[100px]"
                  placeholder="Detaylar..."
                />
              </div>
              <div className="flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setIsTaskModalOpen(false)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                >
                  Oluştur
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Task Details & Comments Modal */}
      {selectedTask && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-2xl shadow-2xl flex flex-col max-h-[90vh]">
            <div className="flex justify-between items-start mb-6">
              <div>
                <h3 className="text-2xl font-bold text-white mb-2">
                  {selectedTask.title}
                </h3>
                <span className="text-xs text-gray-400 bg-gray-700 px-2 py-1 rounded">
                  Oluşturulma:{" "}
                  {new Date(selectedTask.createdAt).toLocaleDateString("tr-TR")}
                </span>
              </div>
              <button
                onClick={() => setSelectedTask(null)}
                className="text-gray-400 hover:text-white"
              >
                ✕
              </button>
            </div>

            <div className="flex-1 overflow-y-auto custom-scrollbar pr-2 mb-6 space-y-6">
              <div>
                <h4 className="text-sm font-semibold text-gray-400 uppercase tracking-wider mb-2">
                  Açıklama
                </h4>
                <p className="text-gray-200 bg-black/20 p-4 rounded-xl border border-white/5 whitespace-pre-wrap">
                  {selectedTask.description || "Açıklama bulunmuyor."}
                </p>
              </div>

              <div>
                <h4 className="text-sm font-semibold text-gray-400 uppercase tracking-wider mb-4">
                  Yorumlar
                </h4>
                <div className="space-y-4 mb-4">
                  {comments.length === 0 ? (
                    <p className="text-gray-500 text-sm">
                      Henüz yorum yapılmamış.
                    </p>
                  ) : (
                    comments.map((comment) => (
                      <div
                        key={comment.id}
                        className="bg-black/20 p-4 rounded-xl border border-white/5"
                      >
                        <div className="flex justify-between items-center mb-2">
                          <span className="font-medium text-blue-400">
                            {comment.userName}
                          </span>
                          <span className="text-xs text-gray-500">
                            {new Date(comment.createdAt).toLocaleString(
                              "tr-TR",
                            )}
                          </span>
                        </div>
                        <p className="text-gray-300 text-sm">
                          {comment.content}
                        </p>
                      </div>
                    ))
                  )}
                </div>

                <form onSubmit={handleAddComment} className="flex space-x-2">
                  <input
                    type="text"
                    required
                    value={newComment}
                    onChange={(e) => setNewComment(e.target.value)}
                    className="flex-1 px-4 py-2 rounded-lg bg-black/40 border border-gray-600 text-white focus:outline-none focus:border-blue-500 text-sm"
                    placeholder="Bir yorum yaz..."
                  />
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors text-sm font-medium"
                  >
                    Gönder
                  </button>
                </form>
              </div>
            </div>

            <div className="pt-4 border-t border-white/10 flex justify-end">
              <button
                onClick={handleDeleteTask}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors text-sm font-medium flex items-center space-x-2"
              >
                <span>🗑️ Görevi Sil</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Column Modal */}
      {editingColumn && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Sütunu Düzenle</h3>
            <form onSubmit={handleUpdateColumn}>
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-1">
                  Sütun Adı
                </label>
                <input
                  type="text"
                  required
                  value={editColumnTitle}
                  onChange={(e) => setEditColumnTitle(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
                />
              </div>
              <div className="flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setEditingColumn(null)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
                >
                  Kaydet
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <style>{`
        .custom-scrollbar::-webkit-scrollbar { width: 6px; }
        .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
        .custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
        .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: rgba(255, 255, 255, 0.2); }
      `}</style>
    </div>
  );
};

export default BoardDetail;
