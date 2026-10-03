import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import type { BoardDto } from '../services/boardService';
import { boardService } from '../services/boardService';
import { useAuth } from '../context/AuthContext';

const BoardList = () => {
  const [boards, setBoards] = useState<BoardDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const { user, logout } = useAuth();
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [isCreating, setIsCreating] = useState(false);

  const [editingBoard, setEditingBoard] = useState<BoardDto | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [isUpdating, setIsUpdating] = useState(false);

  useEffect(() => {
    fetchBoards();
  }, []);

  const fetchBoards = async () => {
    try {
      const data = await boardService.getAll();
      setBoards(data);
    } catch (err: any) {
      setError('Panolar yüklenirken bir hata oluştu.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateBoard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim()) return;
    
    setIsCreating(true);
    try {
      await boardService.create(newTitle, newDescription);
      setIsModalOpen(false);
      setNewTitle('');
      setNewDescription('');
      fetchBoards();
    } catch (err) {
      alert('Pano oluşturulurken bir hata oluştu.');
    } finally {
      setIsCreating(false);
    }
  };

  const handleUpdateBoard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingBoard || !editTitle.trim()) return;
    
    setIsUpdating(true);
    try {
      await boardService.update(editingBoard.id, editTitle, editDescription);
      setEditingBoard(null);
      fetchBoards();
    } catch (err) {
      alert('Pano güncellenirken bir hata oluştu.');
    } finally {
      setIsUpdating(false);
    }
  };

  const handleDeleteBoard = async (e: React.MouseEvent, id: string, title: string) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (window.confirm(`"${title}" panosunu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`)) {
      try {
        await boardService.delete(id);
        fetchBoards();
      } catch (err) {
        alert('Pano silinirken bir hata oluştu.');
      }
    }
  };

  const openEditModal = (e: React.MouseEvent, board: BoardDto) => {
    e.preventDefault();
    e.stopPropagation();
    setEditingBoard(board);
    setEditTitle(board.title);
    setEditDescription(board.description || '');
  };

  return (
    <div className="min-h-screen bg-gray-900 bg-opacity-95 text-white">
      {/* Top Navbar */}
      <nav className="flex items-center justify-between px-8 py-4 bg-white/5 border-b border-white/10 backdrop-blur-md">
        <h1 className="text-2xl font-bold text-white">TaskMaster Pro</h1>
        <div className="flex items-center space-x-6">
          <span className="text-gray-300">Merhaba, <span className="font-semibold text-white">{user?.name}</span></span>
          <button 
            onClick={logout}
            className="px-4 py-2 bg-red-500/20 text-red-300 hover:bg-red-500/30 rounded-lg transition-colors border border-red-500/30"
          >
            Çıkış Yap
          </button>
        </div>
      </nav>

      {/* Main Content */}
      <main className="p-8 max-w-7xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-3xl font-bold">Panolarım</h2>
          <button 
            onClick={() => setIsModalOpen(true)}
            className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors shadow-lg shadow-blue-500/30"
          >
            + Yeni Pano
          </button>
        </div>

        {error && (
          <div className="bg-red-500/20 border border-red-500/50 text-red-200 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {isLoading ? (
          <div className="flex justify-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {boards.length === 0 ? (
              <div className="col-span-full text-center py-12 bg-white/5 rounded-xl border border-white/10">
                <p className="text-gray-400">Henüz hiç panonuz yok. Hemen bir tane oluşturun!</p>
              </div>
            ) : (
              boards.map((board) => (
                <Link 
                  key={board.id} 
                  to={`/boards/${board.id}`}
                  className="group block p-6 rounded-2xl bg-white/5 border border-white/10 hover:bg-white/10 hover:border-white/20 transition-all duration-300 shadow-xl backdrop-blur-sm cursor-pointer hover:-translate-y-1 relative"
                >
                  <div className="absolute top-4 right-4 flex space-x-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button 
                      onClick={(e) => openEditModal(e, board)}
                      className="p-2 bg-gray-800/80 hover:bg-blue-600 rounded-lg text-gray-300 hover:text-white transition-colors backdrop-blur-md"
                      title="Düzenle"
                    >
                      ✏️
                    </button>
                    <button 
                      onClick={(e) => handleDeleteBoard(e, board.id, board.title)}
                      className="p-2 bg-gray-800/80 hover:bg-red-600 rounded-lg text-gray-300 hover:text-white transition-colors backdrop-blur-md"
                      title="Sil"
                    >
                      🗑️
                    </button>
                  </div>
                  <h3 className="text-xl font-bold mb-2 pr-16 group-hover:text-blue-400 transition-colors">{board.title}</h3>
                  <p className="text-gray-400 text-sm line-clamp-2 mt-2">
                    {board.description || 'Açıklama bulunmuyor.'}
                  </p>
                  <div className="mt-6 flex justify-between items-center">
                    <span className="text-xs text-gray-500">
                      Oluşturulma: {new Date(board.createdAt).toLocaleDateString('tr-TR')}
                    </span>
                    <div className="w-8 h-8 rounded-full bg-blue-500/20 flex items-center justify-center text-blue-400 group-hover:bg-blue-500 group-hover:text-white transition-colors">
                      ➔
                    </div>
                  </div>
                </Link>
              ))
            )}
          </div>
        )}
      </main>

      {/* Create Board Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Yeni Pano Oluştur</h3>
            <form onSubmit={handleCreateBoard}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-1">Pano Adı</label>
                <input
                  type="text"
                  required
                  value={newTitle}
                  onChange={(e) => setNewTitle(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
                  placeholder="Örn: Pazarlama Kampanyası"
                />
              </div>
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-1">Açıklama (Opsiyonel)</label>
                <textarea
                  value={newDescription}
                  onChange={(e) => setNewDescription(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500 min-h-[100px]"
                  placeholder="Projenin detayları..."
                />
              </div>
              <div className="flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  disabled={isCreating}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors disabled:opacity-50"
                >
                  {isCreating ? 'Oluşturuluyor...' : 'Oluştur'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Edit Board Modal */}
      {editingBoard && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-gray-800 border border-white/10 rounded-2xl p-6 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Panoyu Düzenle</h3>
            <form onSubmit={handleUpdateBoard}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-1">Pano Adı</label>
                <input
                  type="text"
                  required
                  value={editTitle}
                  onChange={(e) => setEditTitle(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
                />
              </div>
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-300 mb-1">Açıklama (Opsiyonel)</label>
                <textarea
                  value={editDescription}
                  onChange={(e) => setEditDescription(e.target.value)}
                  className="w-full px-4 py-3 rounded-lg bg-black/20 border border-gray-600 text-white focus:outline-none focus:border-blue-500 min-h-[100px]"
                />
              </div>
              <div className="flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={() => setEditingBoard(null)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  disabled={isUpdating}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors disabled:opacity-50"
                >
                  {isUpdating ? 'Kaydediliyor...' : 'Kaydet'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default BoardList;
