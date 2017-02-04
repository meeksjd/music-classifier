% Ricky Rossi
% Feb 3, 2017
% Test various training / test size ratios.
% Script assumes that mlpack is installed and using is running with Octave and Octave-io package installed.
%
% Description:
% This script uses mlpack's LARS method. LARS takes all possible covaries
% and selects the important values to regress on.

clear all;
close all;
pkg load io;
rand("seed",340);

% Known values
BEXTRACT_COLS = 4:32;
lambda_val = 0;

% Load in full data
annotations_with_features = xlsread('../annotations_with_features.xlsx');
dataset_size = length(annotations_with_features);

% Shuffle the data
randomized_table = annotations_with_features(randperm(dataset_size),:);

% Go through various training set sizes
i = 1;
for training_size = 10:10:620
  test_size = dataset_size - training_size;
  
  % Get training and test set
  int_training_set = randomized_table(1:training_size, BEXTRACT_COLS);
  int_responses_set = randomized_table(1:training_size, 3);
  
  pos_training_set = randomized_table(1:training_size, BEXTRACT_COLS);
  pos_responses_set = randomized_table(1:training_size, 2);
  
  test_set = randomized_table(training_size+1:end, BEXTRACT_COLS);
  
  %% Save them to files
  % Intensity
  int_training_set_filename = sprintf('intensity/train/int_train_%i.csv',training_size);
  dlmwrite(int_training_set_filename, int_training_set); % Covariates (X)
  int_responses_filename = sprintf('intensity/train/int_responses_%i.csv', training_size);
  dlmwrite(int_responses_filename, int_responses_set);  % Observations (Y)
  int_test_set_filename = sprintf('intensity/test/int_test_%i.csv',test_size);
  dlmwrite(int_test_set_filename, test_set);
  
  int_predictions_filename = sprintf('intensity/predictions/int_predictions_%i.csv', test_size);
  int_model_filename = sprintf('intensity/models/int_model_%i.xml', training_size);
  int_results_filename = sprintf('intensity/results/int_results_%i.csv', training_size);
  
  % Positivity
  pos_training_set_filename = sprintf('positivity/train/pos_train_%i.csv',training_size);
  dlmwrite(pos_training_set_filename, pos_training_set);  % Covariates (X)
  pos_responses_filename = sprintf('positivity/train/pos_responses_%i.csv', training_size);
  dlmwrite(pos_responses_filename, pos_responses_set);    % Observations (Y)
  pos_test_set_filename = sprintf('positivity/train/pos_test_%i.csv',test_size);
  dlmwrite(pos_test_set_filename, test_set);
  
  pos_predictions_filename = sprintf('positivity/predictions/pos_predictions_%i.csv', test_size);
  pos_model_filename = sprintf('positivity/models/pos_model_%i.xml', training_size);
  pos_results_filename = sprintf('positivity/results/pos_results_%i.csv', training_size);
  
  %% Run linear regression
  % Intensity
  system_cmd = sprintf('mlpack_lars -i %s -r %s -t %s -M %s -o %s -l %0.2f',
                          int_training_set_filename, int_responses_filename, int_test_set_filename, pos_model_filename, int_predictions_filename, lambda_val);
  system(system_cmd);
  int_results = dlmread(int_predictions_filename);
  
  % Positivity
  system_cmd = sprintf('mlpack_lars -i %s -r %s -t %s -M %s -o %s -l %0.2f' ,
                          pos_training_set_filename, pos_responses_filename, pos_test_set_filename, pos_model_filename, pos_predictions_filename, lambda_val);
  system(system_cmd);
  pos_results = dlmread(pos_predictions_filename);
  
  %% Compile results
  % Intensity
  int_results = [randomized_table(training_size+1:end, [1 3 67]) int_results];
  int_results(:,5) = abs(int_results(:,4) - int_results(:,2));
  int_results(:,6) = int_results(:,5) > int_results(:,3);
  
  % Positivity
  pos_results = [randomized_table(training_size+1:end, [1 3 67]) pos_results];
  pos_results(:,5) = abs(pos_results(:,4) - pos_results(:,2));
  pos_results(:,6) = pos_results(:,5) > pos_results(:,3);
  
  % How good is this classifier?
  int_accuracy(i) = sum(int_results(:,6) == 0) / length(int_results(:,6));
  pos_accuracy(i) = sum(pos_results(:,6) == 0) / length(pos_results(:,6));
  
  subplot(3,1,1);
  plot(training_size, int_accuracy(i)); hold on;
  title('Intensity Accuracy')
  xlabel('Training Size')
  axis([0 700 0 1])
  
  subplot(3,1,2)
  plot(training_size, pos_accuracy(i)); hold on;
  title('Positivity Accuracy')
  xlabel('Training Size')
  axis([0 700 0 1])
  
  subplot(3,1,3)
  plot(training_size, int_accuracy(i) * pos_accuracy(i)); hold on;
  title('Int * Pos Accuracy');
  xlabel('Training Size')
  axis([0 700 0 1])
  
  
  int_results(:,7) = int_accuracy(i);
  dlmwrite(int_results_filename, int_results);
  
  pos_results(:,7) = pos_accuracy(i);
  dlmwrite(pos_results_filename, pos_results); 
  
  i = i + 1;
end