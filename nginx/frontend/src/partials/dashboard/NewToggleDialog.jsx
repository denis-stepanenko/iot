import React, { useState } from 'react'
import { Button, Dialog, DialogTitle, DialogContentText, TextField, DialogActions } from '@mui/material'
import DialogContent from '@mui/material/DialogContent';
import Alert from '@mui/material/Alert';

export default function NewToggleDialog({ items, onAdd }) {
  const [open, setOpen] = useState(false);
  const [commonError, setCommonError] = useState('')
  const [nameError, setNameError] = useState('')
  const [topicError, setTopicError] = useState('')

  const handleClickOpen = () => {
    setOpen(true);
    setCommonError('')
    setNameError('');
    setTopicError('');
  };

  const handleClose = () => {
    setOpen(false);
  };

  return (
    <>
      <Button variant="text" onClick={handleClickOpen}>Добавить переключатель</Button>

      <Dialog
        open={open}
        onClose={handleClose}
        PaperProps={{
          component: 'form',
          onSubmit: (event) => {
            event.preventDefault();
            const formData = new FormData(event.currentTarget);
            const formJson = Object.fromEntries(formData.entries());

            const name = formJson.name;
            const topic = formJson.topic;

            setCommonError('')
            setNameError('');
            setTopicError('');

            let hasErrors = false;

            if (name === '') {
              setNameError('Введите название');
              hasErrors = true;
            }

            if (topic === '') {
              setTopicError('Введите тему');
              hasErrors = true;
            }

            if (items.some(x => x.topic === topic || x.name === name)) {
              setCommonError('Элемент с таким названием или темой уже существует');
              hasErrors = true;
            }


            if (!hasErrors) {
              onAdd({ $type: 'toggle', title: name, topic: topic, value: false });

              handleClose();
            }


          },
        }}
      >
        <DialogTitle>Новый переключатель</DialogTitle>
        <DialogContent>
          <DialogContentText>В качестве передаваемых значений используются "0" и "1".</DialogContentText>

          {commonError && <Alert severity="error">{commonError}</Alert>}

          <TextField
            autoFocus
            margin="dense"
            id="name"
            name="name"
            label="Название"
            type="text"
            fullWidth
            variant="standard"
            {...nameError === '' ? {} : { helperText: nameError, error: true }}
          />

          <TextField
            margin="dense"
            id="topic"
            name="topic"
            label="Тема"
            type="text"
            fullWidth
            variant="standard"
            {...topicError === '' ? {} : { helperText: topicError, error: true }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Отмена</Button>
          <Button type="submit">Добавить</Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
