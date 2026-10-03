import api from './api';

export const columnService = {
  create: async (boardId: string, title: string): Promise<void> => {
    await api.post(`/boards/${boardId}/columns`, { title });
  },
  
  update: async (id: string, title: string): Promise<void> => {
    await api.put(`/columns/${id}`, { title });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/columns/${id}`);
  }
};
