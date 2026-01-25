import React, { useState } from 'react';
import { Dialog, DialogTitle, DialogContent, TextField, DialogActions, Button } from '@mui/material';


/* AddRoomModal Component
   Provides a dialog interface for the Administrator to create a new room(organizational unit)
   within the smart home.This component handles local state for the room name and
   triggers the creation process in the parent container.
*/
function AddRoomModal({ open, onClose, onAdd }) {
    const [name, setName] = useState('');

    const handleAdd = () => {
        if (name.trim()) {
            onAdd({ name });
            setName('');
            onClose();
        }
    };

    return (
        <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
            <DialogTitle>Add a New Room</DialogTitle>
            <DialogContent>
                <TextField
                    autoFocus
                    margin="dense"
                    id="name"
                    label="Room Name"
                    type="text"
                    fullWidth
                    variant="standard"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && handleAdd()}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button onClick={handleAdd} variant="contained">Add</Button>
            </DialogActions>
        </Dialog>
    );
}

export default AddRoomModal;