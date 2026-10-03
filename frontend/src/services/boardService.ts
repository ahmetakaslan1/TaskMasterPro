import api from './api';

export interface TaskDto {
  id: string;
  columnId: string;
  title: string;
  description: string;
  color: string;
  order: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface ColumnDto {
  id: string;
  boardId: string;
  title: string;
  order: number;
  tasks: TaskDto[];
}

export interface BoardDetailDto {
  id: string;
  title: string;
  description: string;
  ownerId: string;
  createdAt: string;
  columns: ColumnDto[];
}

export interface BoardDto {
  id: string;
  title: string;
  description: string;
  ownerId: string;
  createdAt: string;
}

export const boardService = {
  getAll: async (): Promise<BoardDto[]> => {
    const response = await api.get('/boards');
    return response.data.data;
  },
  
  getById: async (id: string): Promise<BoardDetailDto> => {
    const response = await api.get(`/boards/${id}`);
    return response.data.data;
  },

  create: async (title: string, description: string = ''): Promise<BoardDetailDto> => {
    const response = await api.post('/boards', { title, description });
    return response.data.data;
  },

  update: async (id: string, title: string, description: string = ''): Promise<void> => {
    await api.put(`/boards/${id}`, { title, description });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/boards/${id}`);
  }
};
