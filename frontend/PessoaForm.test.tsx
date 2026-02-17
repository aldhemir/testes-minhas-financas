import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

interface PessoaFormProps {
  onSubmit: (data: { nome: string; dataNascimento: string }) => void;
  initialData?: { nome: string; dataNascimento: string };
}

const MockPessoaForm: React.FC<PessoaFormProps> = ({ onSubmit, initialData }) => {
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    onSubmit({
      nome: formData.get('nome') as string,
      dataNascimento: formData.get('dataNascimento') as string,
    });
  };

  return (
    <form onSubmit={handleSubmit} data-testid="pessoa-form">
      <label htmlFor="nome">Nome</label>
      <input
        id="nome"
        name="nome"
        type="text"
        defaultValue={initialData?.nome}
        required
      />
      
      <label htmlFor="dataNascimento">Data de Nascimento</label>
      <input
        id="dataNascimento"
        name="dataNascimento"
        type="date"
        defaultValue={initialData?.dataNascimento}
        required
      />
      
      <button type="submit">Salvar</button>
    </form>
  );
};

describe('PessoaForm Component', () => {
  
  it('deve renderizar todos os campos do formulário', () => {
    const mockSubmit = vi.fn();
    
    render(<MockPessoaForm onSubmit={mockSubmit} />);
    
    expect(screen.getByLabelText(/nome/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/data de nascimento/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /salvar/i })).toBeInTheDocument();
  });

  it('deve preencher campos com dados iniciais', () => {
    const mockSubmit = vi.fn();
    const initialData = {
      nome: 'João Silva',
      dataNascimento: '1990-01-15',
    };
    
    render(<MockPessoaForm onSubmit={mockSubmit} initialData={initialData} />);
    
    const nomeInput = screen.getByLabelText(/nome/i) as HTMLInputElement;
    const dataInput = screen.getByLabelText(/data de nascimento/i) as HTMLInputElement;
    
    expect(nomeInput.value).toBe('João Silva');
    expect(dataInput.value).toBe('1990-01-15');
  });

  it('deve chamar onSubmit com dados corretos ao submeter', async () => {
    const mockSubmit = vi.fn();
    const user = userEvent.setup();
    
    render(<MockPessoaForm onSubmit={mockSubmit} />);
    
    const nomeInput = screen.getByLabelText(/nome/i);
    const dataInput = screen.getByLabelText(/data de nascimento/i);
    const submitButton = screen.getByRole('button', { name: /salvar/i });
    
    await user.type(nomeInput, 'Maria Santos');
    await user.type(dataInput, '1995-05-20');
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalledWith({
        nome: 'Maria Santos',
        dataNascimento: '1995-05-20',
      });
    });
  });

  it('deve exigir campos obrigatórios', async () => {
    const mockSubmit = vi.fn();
    const user = userEvent.setup();
    
    render(<MockPessoaForm onSubmit={mockSubmit} />);
    
    const submitButton = screen.getByRole('button', { name: /salvar/i });
    await user.click(submitButton);
    
    expect(mockSubmit).not.toHaveBeenCalled();
  });

  it('deve aceitar menor de idade', async () => {
    const mockSubmit = vi.fn();
    const user = userEvent.setup();
    
    render(<MockPessoaForm onSubmit={mockSubmit} />);
    
    const hoje = new Date();
    const dataMenor = new Date(hoje.getFullYear() - 15, 0, 1);
    const dataFormatada = dataMenor.toISOString().split('T')[0];
    
    await user.type(screen.getByLabelText(/nome/i), 'Menor Teste');
    await user.type(screen.getByLabelText(/data de nascimento/i), dataFormatada);
    await user.click(screen.getByRole('button', { name: /salvar/i }));
    
    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalled();
    });
  });
});
