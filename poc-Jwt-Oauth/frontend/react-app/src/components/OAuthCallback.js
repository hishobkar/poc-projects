import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const OAuthCallback = () => {
  const [error, setError] = useState('');
  const { handleOAuthCallback } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get('token');
    const refreshToken = params.get('refreshToken');
    const errorParam = params.get('error');

    if (errorParam) {
      setError(errorParam);
      setTimeout(() => navigate('/login'), 3000);
      return;
    }

    if (token) {
      handleOAuthCallback(token, refreshToken);
      navigate('/dashboard');
    } else {
      setError('No token received');
      setTimeout(() => navigate('/login'), 3000);
    }
  }, [location, handleOAuthCallback, navigate]);

  return (
    <div className="oauth-callback">
      {error ? (
        <div className="error">
          <h2>Authentication Error</h2>
          <p>{error}</p>
          <p>Redirecting to login...</p>
        </div>
      ) : (
        <div className="loading">
          <h2>Processing OAuth Login...</h2>
          <div className="spinner"></div>
        </div>
      )}
    </div>
  );
};

export default OAuthCallback;