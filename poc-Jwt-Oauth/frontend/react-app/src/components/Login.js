import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Auth.css';

const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, oauthLogin } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const result = await login(username, password);
    if (result.success) {
      navigate('/dashboard');
    } else {
      setError(result.message);
    }
    setLoading(false);
  };

  const handleOAuth = (provider) => {
    oauthLogin(provider);
  };

  return (
    <div className="auth-container">
      <div className="auth-box">
        <h2>Login</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Username or Email</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              placeholder="Enter username or email"
            />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Enter password"
            />
          </div>
          {error && <div className="error-message">{error}</div>}
          <button type="submit" disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <div className="divider">
          <span>or continue with</span>
        </div>

        <div className="oauth-buttons">
          <button 
            className="google-btn"
            onClick={() => handleOAuth('google')}
          >
            <i className="fab fa-google"></i>
            Google
          </button>
          <button 
            className="facebook-btn"
            onClick={() => handleOAuth('facebook')}
          >
            <i className="fab fa-facebook"></i>
            Facebook
          </button>
        </div>

        <div className="auth-footer">
          <p>Don't have an account? <Link to="/register">Register</Link></p>
        </div>

        <div className="auth-info">
          <p><strong>Demo Credentials:</strong></p>
          <p>Username: testuser</p>
          <p>Password: Test123!</p>
        </div>
      </div>
    </div>
  );
};

export default Login;