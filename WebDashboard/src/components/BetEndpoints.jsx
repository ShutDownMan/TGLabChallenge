import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { API_HOST } from '../config/api';
import { Card, CardContent, Typography, TextField, Button, Grid, Select, MenuItem } from '@mui/material';
import ResponseDisplay from './ResponseDisplay';

function BetEndpoints({ jwtToken }) {
  const [response, setResponse] = useState(null);
  const [status, setStatus] = useState(null);
  const [responseTime, setResponseTime] = useState(null);
  const [selectedWallet, setSelectedWallet] = useState('');
  const [selectedGame, setSelectedGame] = useState('7558398b-a987-4b88-9010-c026306d3535');
  const [playerProfile, setPlayerProfile] = useState(null);
  const [betId, setBetId] = useState('');
  const [availableBets, setAvailableBets] = useState([]);
  const [availableGames, setAvailableGames] = useState([
    { id: '7558398b-a987-4b88-9010-c026306d3535', name: 'Default Game' }
  ]);
  const [betAmount, setBetAmount] = useState(100);

  const fetchProfile = async () => {
    try {
      const res = await axios.get(`${API_HOST}/api/Player/profile`, {
        headers: { Authorization: `Bearer ${jwtToken}` },
      });
      setPlayerProfile(res.data);
      if (res.data.wallets?.length > 0) {
        setSelectedWallet(res.data.wallets[0].id);
      }
    } catch (error) {
      console.error('Failed to fetch profile:', error);
    }
  };

  const fetchBets = async () => {
    try {
      const res = await axios.get(`${API_HOST}/api/Player/bets`, {
        headers: { Authorization: `Bearer ${jwtToken}` },
      });
      setAvailableBets(res.data);
      if (res.data.length > 0) {
        setBetId(res.data[0].id);
      }
    } catch (error) {
      console.error('Failed to fetch player bets:', error);
    }
  };

  useEffect(() => {
    fetchProfile();
  }, [jwtToken]);

  useEffect(() => {
    fetchBets();
  }, [jwtToken]);

  useEffect(() => {
    const fetchGames = async () => {
      try {
        const res = await axios.get(`${API_HOST}/api/Games`, {
          headers: { Authorization: `Bearer ${jwtToken}` },
        });
        setAvailableGames((prevGames) => [
          ...prevGames,
          ...res.data.filter((game) => !prevGames.some((g) => g.id === game.id))
        ]);
        if (res.data.length > 0 && !selectedGame) {
          setSelectedGame(res.data[0].id);
        }
      } catch (error) {
        console.error('Failed to fetch games:', error);
      }
    };
    fetchGames();
  }, [jwtToken]);

  const handleRequest = async (endpoint, method, data = null) => {
    try {
      const startTime = performance.now();
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
        data,
      };
      const res = await axios(config);
      const endTime = performance.now();
      setResponse(res.data);
      setStatus(res.status);
      setResponseTime(Math.round(endTime - startTime));
      refreshData();
    } catch (error) {
      setResponse(error.response?.data || error.message);
      setStatus(error.response?.status || 'Error');
      setResponseTime(null);
    }
  };

  const refreshData = async () => {
    await Promise.all([fetchProfile(), fetchBets()]);
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Place Bet
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Select
              fullWidth
              value={selectedWallet}
              onChange={(e) => setSelectedWallet(e.target.value)}
              displayEmpty
            >
              <MenuItem value="">Select Wallet</MenuItem>
              {playerProfile?.wallets?.map((wallet) => (
                <MenuItem key={wallet.id} value={wallet.id}>
                  {wallet.name || `Wallet ${wallet.id}`}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid item xs={12}>
            <Select
              fullWidth
              value={selectedGame}
              onChange={(e) => setSelectedGame(e.target.value)}
              displayEmpty
            >
              <MenuItem value="">Select Game</MenuItem>
              {availableGames?.map((game) => (
                <MenuItem key={game.id} value={game.id}>
                  {game.name || `Game ${game.id}`}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Bet Amount"
              fullWidth
              type="number"
              value={betAmount}
              onChange={(e) => setBetAmount(e.target.value)}
            />
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="primary"
              onClick={() =>
                handleRequest('/api/Bet', 'POST', {
                  walletId: selectedWallet,
                  gameId: selectedGame,
                  amount: Number(betAmount),
                  currencyId: 1,
                })
              }
              disabled={!selectedWallet || !selectedGame}
            >
              Place Bet
            </Button>
          </Grid>
        </Grid>
        <Typography variant="h6" gutterBottom style={{ marginTop: '20px' }}>
          Manage Bets
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Select
              fullWidth
              value={betId}
              onChange={(e) => setBetId(e.target.value)}
              displayEmpty
            >
              <MenuItem value="">Select Bet</MenuItem>
              {availableBets?.map((bet) => (
                <MenuItem key={bet.id} value={bet.id}>
                  {`Bet ${bet.id} - ${bet.status}`}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="secondary"
              onClick={() =>
                handleRequest(`/api/Bet/${betId}/cancel`, 'POST', {
                  cancelReason: 'User requested cancellation',
                })
              }
              disabled={!betId}
            >
              Cancel Bet
            </Button>
          </Grid>
          <Grid item xs={12}>
            <Button
              variant="contained"
              color="success"
              onClick={() => handleRequest(`/api/Bet/${betId}/settle`, 'POST')}
              disabled={!betId}
            >
              Settle Bet
            </Button>
          </Grid>
        </Grid>
        <ResponseDisplay response={response} status={status} responseTime={responseTime} />
      </CardContent>
    </Card>
  );
}

export default BetEndpoints;
