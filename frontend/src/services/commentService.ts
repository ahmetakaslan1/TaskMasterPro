import api from './api';

export interface CommentDto {
  id: string;
  taskId: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
}

export const commentService = {
  getByTaskId: async (taskId: string): Promise<CommentDto[]> => {
    const response = await api.get(`/tasks/${taskId}/comments`);
    return response.data.data;
  },
  
  create: async (taskId: string, content: string): Promise<void> => {
    await api.post(`/tasks/${taskId}/comments`, { content });
  },

  update: async (id: string, content: string): Promise<void> => {
    await api.put(`/comments/${id}`, { content });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/comments/${id}`);
  }
};
