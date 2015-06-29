function [ decision ] = albumKnn( userWorkSpace, k, numOfParameters )

fileToLearn = strcat(userWorkSpace,'\\Learn.txt');
fileToDecide = strcat(userWorkSpace,'\\Decide.txt');
%profilePath = strcat(userWorkSpace,'\\profilerbf.mat');
%profilePath = strcat(userWorkSpace,'\\profile');
%profilePath = strcat(profilePath, kernel);
%profilePath = strcat(profilePath, '.mat');
resultFilePath = strcat(userWorkSpace,'\\Result.txt');

%load(profilePath,'svmStruct');

M = csvread(fileToLearn); % read comma separated values
MLearn = M(1:end,1:numOfParameters); % parameters to learn from
group  = M(1:end,(numOfParameters+1));  % last column in file 1-good image, 0-bad image


MToDecide = csvread(fileToDecide); % read comma separated values
MToDecide = MToDecide(1,1:numOfParameters); %parameters to decide on

%group     = normr(group);
MLearn     = normr(MLearn);
MToDecide  = normr(MToDecide);
%MLearn    = normalizeMatrix(svmStruct,MLearn);
%MToDecide = normalizeMatrix(svmStruct,MToDecide);


%mdl = ClassificationKNN.fit(MLearn, group, 'NumNeighbors', k);
%[decision, confidence] = predict(mdl, MToDecide);

%decide about new picture
decision = knnclassify(MToDecide, MLearn, group, k);

%for 2014a version
%mdl = fitcknn(MLearn, group, 'NumNeighbors', k);
%decision = predict(mdl,MToDecide);

resultFile = fopen(resultFilePath, 'w');
fprintf(resultFile, '%d', decision);
%fprintf(resultFile, '%d', confidence);
fclose(resultFile);

end

function [ result ] = normalizeMatrix(svmStruct, matrix)
  for c = 1:size(matrix, 2)
    matrix(:,c) = svmStruct.ScaleData.scaleFactor(c) * ...
    (matrix(:,c) +  svmStruct.ScaleData.shift(c));
  end
  result = matrix;
end