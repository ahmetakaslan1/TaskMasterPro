import api from './api';

export const taskService = {
  create: async (columnId: string, title: string, description: string = '', color: string = '#3b82f6'): Promise<void> => {
    await api.post(`/columns/${columnId}/tasks`, { title, description, color });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/tasks/${id}`);
  },

  move: async (id: string, targetColumnId: string, newIndex: number): Promise<void> => {
    await api.patch(`/tasks/${id}/move`, { targetColumnId, newIndex });
  }
};
